namespace Wordle_CSharp.Sources
{
    internal class SaveState
    {
        public SaveState(string word, string[] guesses, int guess_count) 
        {
            Word = word;
            Guesses = guesses;
            GuessCount = guess_count;
        }

        public string Word;
        public string[] Guesses;
        public int GuessCount;
    }
}
