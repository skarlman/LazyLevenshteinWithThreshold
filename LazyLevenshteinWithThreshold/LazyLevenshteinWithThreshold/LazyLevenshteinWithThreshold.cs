using System;

namespace LazyLevenshteinWithThreshold
{
    /// <summary>
    ///     Lazy implementation of Levenshtein string distance,
    /// 
    ///     Complexity:
    ///     O(|a|*(Dist(a, b)))
    ///
    ///     If same strings:
    ///     O(|a|)
    ///
    ///     If completely different strings:
    ///     O(|a|*|b|)   (just as the regular dynamic programming solution)
    ///
    ///     The algorithm supports thresholding, hence the worst case time complexity is  O(|a|*threshold)
    ///     (If threshold is reached, distance calculation stops and int.MaxValue is returned)
    ///
    ///     C# Implementation and addition of thresholding by Karl Tillström
    ///     https://www.codeinthenorth.com
    ///     Developed in Master Thesis "Java exception matching in real time using fuzzy logic" (2010)
    ///     https://odr.chalmers.se/handle/20.500.12380/122239
    ///
    ///     Original Algorithm (without thresholding) by:
    ///     L.Allison,
    ///     Department of Computer Science,
    ///     Monash University,
    ///     Australia 3168.
    ///     December 1991, Revised May 1992
    ///     http://www.csse.monash.edu.au/~lloyd/tildeStrings/Alignment/92.IPL.html
    /// </summary>
    public class LazyLevenshteinWithThreshold
    {
        private static readonly LazyLevenshteinWithThreshold Calculator = new LazyLevenshteinWithThreshold();

        /// <summary>
        ///     Calculates the Levenshtein distance between the strings, up to the given threshold
        /// </summary>
        /// <param name="firstString"></param>
        /// <param name="secondString"></param>
        /// <param name="threshold">The threshold to stop calculating at</param>
        /// <returns>The Levenshtein distance between the strings, or Int.MaxValue if threshold is reached</returns>
        public static int CalculateDistance(string firstString, string secondString, int threshold = int.MaxValue)
        {
            return Calculator.CalculateStringDistance(firstString.ToCharArray(), secondString.ToCharArray(), threshold);
        }


        /// <summary>
        ///     Calculates the Levenshtein distance between the strings, up to the given threshold
        /// </summary>
        /// <param name="firstString"></param>
        /// <param name="secondString"></param>
        /// <param name="threshold">The threshold to stop calculating at</param>
        /// <returns>The Levenshtein distance between the strings, or Int.MaxValue if threshold is reached</returns>
        public int CalculateStringDistance(string firstString, string secondString, int threshold = int.MaxValue)
        {
            return CalculateStringDistance(firstString.ToCharArray(), secondString.ToCharArray(), threshold);
        }

        /// <summary>
        ///     Calculates the Levenshtein distance between the strings, up to the given threshold
        /// </summary>
        /// <param name="firstString"></param>
        /// <param name="secondString"></param>
        /// <param name="threshold">The threshold to stop calculating at</param>
        /// <returns>The Levenshtein distance between the strings, or Int.MaxValue if threshold is reached</returns>
        public int CalculateStringDistance(char[] firstString, char[] secondString, int threshold = int.MaxValue)
        {
            // Diagonal from the top-left element
            var topLeftDiagonal = new Diagonal(firstString, secondString, null, 0, threshold);
            Diagonal resultDiagonal;


            // The diagonal containing the bottom right element, i.e. the resulting distance
            var resultDiagonalIndex = secondString.Length - firstString.Length;

            // Move to the diagonal that will contain the final result
            if (resultDiagonalIndex >= 0)
            {
                resultDiagonal = topLeftDiagonal;
                for (var i = 0; i < resultDiagonalIndex; i++) resultDiagonal = resultDiagonal.GetFurther();
            }
            else
            {
                resultDiagonal = topLeftDiagonal.GetCloser();
                for (var i = 0; i < ~resultDiagonalIndex; i++) resultDiagonal = resultDiagonal.GetFurther();
            }

            return resultDiagonal.Get(Math.Min(firstString.Length, secondString.Length));
        }


        /// <summary>
        ///     Returns the calculated matrix
        /// </summary>
        /// <param name="firstString"></param>
        /// <param name="secondString"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public int[][] GetDistanceMatrix(char[] firstString, char[] secondString, int threshold)
        {
            var result = new int[firstString.Length + 1][];

            for (var i = 0; i < firstString.Length + 1; i++)
            {
                result[i] = new int[secondString.Length + 1];

                for (var j = 0; j < secondString.Length + 1; j++) result[i][j] = int.MinValue;
            }


            var topLeftDiagonal = new Diagonal(firstString, secondString, null, 0, threshold);
            Diagonal resultDiagonal;


            var requestedDiagonalNumber = secondString.Length - firstString.Length;

            if (requestedDiagonalNumber >= 0)
            {
                resultDiagonal = topLeftDiagonal;
                for (var i = 0; i < requestedDiagonalNumber; i++) resultDiagonal = resultDiagonal.GetFurther();
            }
            else
            {
                resultDiagonal = topLeftDiagonal.GetCloser();
                for (var i = 0; i < ~requestedDiagonalNumber; i++) resultDiagonal = resultDiagonal.GetFurther();
            }

            resultDiagonal.Get(Math.Min(firstString.Length, secondString.Length));

            //Fill in main diagonal
            for (var i = 0; i < topLeftDiagonal.CostElementPointer; i++) result[i][i] = topLeftDiagonal.CostElements[i];


            //Fill in above
            var currentDiag = topLeftDiagonal.NextDiagonalFurtherFromMiddle;
            while (currentDiag != null)
            {
                for (var i = 0; i < currentDiag.CostElementPointer; i++)
                    result[i][i + currentDiag.Index] = currentDiag.CostElements[i];

                currentDiag = currentDiag.NextDiagonalFurtherFromMiddle;
            }

            //fill in below
            currentDiag = topLeftDiagonal.NextDiagonalCloserToMiddle;
            while (currentDiag != null)
            {
                for (var i = 0; i < currentDiag.CostElementPointer; i++)
                    result[i + -1 * currentDiag.Index][i] = currentDiag.CostElements[i];

                //saved transposed!
                currentDiag = currentDiag.NextDiagonalFurtherFromMiddle;
            }

            return result;
        }

        /// <summary>
        ///     The Levenshtein dynamic programming cost distance matrix, in terms of its diagonals instead of rows
        ///     The final distance is retrieved by calculating the diagonal with the value in the lower right corner.
        ///     Needed intermediate values from the diagonals above or under are calculated lazily if needed.
        ///     This way, we don't need to calculate the whole matrix, unless the strings are completely different.
        ///
        ///     Complexity: O(|a|*Distance(a,b))
        /// </summary>
        private class Diagonal
        {
            private readonly char[] _strLeft;
            private readonly char[] _strTop;

            /* Stops the searching at the given threshold and returns Integer.MAX_VALUE
             * This is to not waste time on comparisons that will not be used anyway
             */
            private readonly int _threshold;

            // The contents of the diagonal, edit distances 
            public readonly int[] CostElements;
            /* A diagonal contains all elements in a diagonal of the dynamic programming "algorithm representation"
             * The middle diagonal is indexed 0, all diagonals above: 1...n and all below: -1...-m 
             */

            public readonly int Index;
            public int CostElementPointer;

            public Diagonal NextDiagonalCloserToMiddle;
            public Diagonal NextDiagonalFurtherFromMiddle;

            public Diagonal(char[] string1, char[] string2, Diagonal diagonalCloserToMiddle, int index, int threshold)
            {
                if (Math.Abs(index) > string2.Length)
                    throw new Exception("This cannot happen, you have a bug");


                Index = index;
                _threshold = threshold;

                _strLeft = string1;
                _strTop = string2;

                // The number of cost elements depends on which diagonal it is
                if (index == 0)
                    CostElements = new int[Math.Min(_strLeft.Length, _strTop.Length) + 2];
                else if (index < 0)
                    //Matrix is transposed, hence strTop is to be used here as well!
                    CostElements = new int[_strTop.Length + 2 + index]; //index is negative, hence +
                else
                    CostElements = new int[_strTop.Length + 2 - index];

                NextDiagonalCloserToMiddle = diagonalCloserToMiddle;

                /* First cost in diagonal = |index|
                 *   |
                 *   V
                 *     a b a b
                 *   0 1 2 3 4  <---
                 * a 1 0 1 2 3
                 * b 2 1 0 1 2
                 * b 3 2 1 1 1
                 * c 4 3 2 2 2
                 * 
                 */

                CostElements[CostElementPointer++] = Math.Abs(index);
            }


            /// <summary>
            ///     Returns the diagonal closer to the middle from this current one.
            ///     If it does not exist, it is created (this is a lazy algorithm).
            /// </summary>
            public Diagonal GetCloser()
            {
                if (NextDiagonalCloserToMiddle != null)
                    return NextDiagonalCloserToMiddle;

                if (Index != 0)
                    throw new Exception("This should not happen, you have a bug");

                /* The Diagonals below the middle one is stored with strLeft and strTop switched, to have them 
                   * practically in the upper half of a transposed matrix. This way the same Diagonal class with 
                   * its modifying members can be used for both upper and lower Diagonals.
                   *  
                   * This is very important for the implementation of min3() later on!
                */
                NextDiagonalCloserToMiddle = new Diagonal(_strTop, _strLeft, this, -1, _threshold);

                return NextDiagonalCloserToMiddle;
            }


            /// <summary>
            ///     Returns the diagonal further from the middle from this current one.
            ///     If it does not exist, it is created (this is a lazy algorithm).
            /// </summary>
            /// <returns></returns>
            public Diagonal GetFurther()
            {
                if (NextDiagonalFurtherFromMiddle != null)
                    return NextDiagonalFurtherFromMiddle;

                // Regarding index: above diagonals have positive indices and belows have negative
                NextDiagonalFurtherFromMiddle = new Diagonal(_strLeft, _strTop, this,
                    Index >= 0 ? Index + 1 : Index - 1, _threshold);

                return NextDiagonalFurtherFromMiddle;
            }


            /* --- Retrieval functions ---
             *   a  b  c  d
             * a .  .  .  .    C: Current element
             * b .  TL T  .    L: Element to the Left of C 
             * c .  L  C  .    TL: Element to the Top Left of C
             * d .  .  .  .    T: Element to the top of C
             *
             * The major cornerstone in the algorithm is that if  (L < TL)  then  (T <= L)
             * Hence, if L < TL, we con't need to calculate T, leading to whole diagonals not being calculated
             */


            /// <summary>
            ///     Returns cost element j from the Diagnoal (calculates it if necessary)
            /// </summary>
            /// <param name="position">The index of the requested cost element</param>
            /// <returns>The cost in the position in the diagonal</returns>
            public int Get(int position)
            {
                if (!(position >= 0 && position <= _strTop.Length - Math.Abs(Index) && position <= _strLeft.Length))
                    throw new Exception("This should not happen, you have a bug");

                if (position < CostElementPointer) return CostElements[position];


                // Calculate values needed up to the requested position
                var requestedElement = CostElements[CostElementPointer - 1];

                for (var ctr = position + 1 - CostElementPointer; --ctr >= 0;)
                {
                    var topLeftElement = requestedElement;

                    /* Levenshtein rules for cost calculations:
                     * 
                     *  if currentElement == topLeftElement  --> currentElement = topLeftElement
                     *  else
                     *  currentElement = min3(leftElement+1, topLeftElement+1, topElement+1)
                     *  
                     *  The trick with this algorithm is that if leftElement<topLeftElement -> leftElement <= topElement
                     *  and therefore topElement is not needed to compute. This is where the lazy algorithm beats the dynamic programming one
                     *  
                     *  see L. Allison, Lazy Dynamic-Programming can be Eager
                     *  Inf. Proc. Letters 43(4) pp207-212, Sept' 1992
                     *  http://www.csse.monash.edu.au/~lloyd/tildeStrings/Alignment/92.IPL.html
                     *  
                     */

                    // costElementPointer is the same as number of elements in the diagonal (1 indexed)
                    if (_strLeft[CostElementPointer - 1] == _strTop[Math.Abs(Index) + CostElementPointer - 1])
                    {
                        // if chars are equal, it cannot be lower cost than the top left element
                        requestedElement = topLeftElement;
                    }
                    else
                    {
                        /* Computes min3 (Left, TopLeft, Top)
                         * but does not always evaluate Top
                         * this makes it O(|a|*D(strLeft,strTop))
                         */

                        var myLeftElement = GetCloser().Get(Index == 0 ? CostElementPointer - 1 : CostElementPointer);
                        if (myLeftElement < topLeftElement)
                            requestedElement = 1 + myLeftElement;
                        else
                            //Compare with top element (costly)
                            requestedElement = 1 + Math.Min(topLeftElement, GetFurther().Get(CostElementPointer - 1));
                    }

                    // store results to avoid recalculating
                    CostElements[CostElementPointer++] = requestedElement;


                    // Stop calculating if we are above threshold
                    if (requestedElement > _threshold)
                        return int.MaxValue;
                }

                return requestedElement;
            }
        }
    }
}
