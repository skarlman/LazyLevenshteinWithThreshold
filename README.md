# LazyLevenshteinWithThreshold
A C# implementation of the Levenshtein string distance, using lazy evaluation of its diagonals

## Usage
`LazyLevenshteinWithThreshold.CalculateDistance(string1, string2)`

or 

`new LazyLevenshteinWithThreshold().CalculateStringDistance(string1, string2)`

## Complexity:
O(|a|*(Dist(a, b)))

## If same strings:
O(|a|)

## If completely different strings:
O(|a|*|b|)   
(just as the regular dynamic programming solution)


## Thresholding
The algorithm supports thresholding, hence the worst case time complexity is  O(|a|*threshold)
(If threshold is reached, distance calculation stops and int.MaxValue is returned)


## Note on Time complexity
Even if this algorithm has a better time complexity, it is vastly slower than regular Levenshtein Dynamic programming algorithms on small strings due to the fact that it needs to initialize objects


## Implementation and addition of thresholding by Karl Tillström
https://www.codeinthenorth.com

Developed in Master Thesis "Java exception matching in real time using fuzzy logic" (2010)
https://odr.chalmers.se/handle/20.500.12380/122239


## Original Algorithm (without thresholding) by:
L.Allison,
Department of Computer Science,
Monash University,
Australia 3168.
December 1991, Revised May 1992
http://www.csse.monash.edu.au/~lloyd/tildeStrings/Alignment/92.IPL.html