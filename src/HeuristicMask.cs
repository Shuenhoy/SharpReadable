namespace SharpReadable
{
    class HeuristicMask : IMask
    {
        public bool[] GetMask(string page)
        {
            var mask = new bool[page.Length];

            var words = page.Split(' ');
            var wordPositions = new int[words.Length];
            if (words.Length > 0)
            {
                wordPositions[0] = 0;
                for (int i = 1; i < words.Length; i++)
                {
                    wordPositions[i] = wordPositions[i - 1] + words[i - 1].Length + 1;
                }

                foreach (var (wordPosition, word) in Enumerable.Zip(wordPositions, words))
                {
                    var wordLength = word.Length;
                    var markLength = (int)Math.Ceiling(wordLength * 0.5);
                    for (int i = 0; i < markLength; i++)
                    {
                        mask[wordPosition + i] = true;
                    }
                }
            }
            return mask;
        }
    }
}