namespace Wordle_CSharp.Sources
{
    internal class Wordle
    {
        public enum GameStatus { Play, Quit };

        public Wordle()
        {
            m_State = new("save_state.json");
            m_BoxHeight = WordleState.MAX_GUESSES + 4;
        }

        public GameStatus Play()
        {
            PrintBoard();

            WordleState.GuessStatus guess_status = WordleState.GuessStatus.Null;
            while (guess_status != WordleState.GuessStatus.Success && guess_status != WordleState.GuessStatus.MaxGuesses)
            {
                Console.Write("Guess: ");
                string guess = Console.ReadLine() ?? "";
                if (guess == "-1")
                {
                    m_State.SaveToFile("save_state.json");
                    return GameStatus.Quit;
                }

                guess_status = m_State.Guess(guess);
                if (guess_status != WordleState.GuessStatus.Failure)
                {
                    Console.WriteLine(WordleState.GUESS_MESSAGES[(int)guess_status]);
                    if (guess_status == WordleState.GuessStatus.MaxGuesses) Console.WriteLine($"Word was: {m_State.Word}");
                    continue;
                }

                UpdateBoard();
            }

            m_State = new();
            return GameStatus.Play;
        }

        private void PrintBoard()
        {
            ConsoleColor colour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            (int left, int top) = Console.GetCursorPosition();
            top = int.Max(top, m_BoxHeight + 1);
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("-------------------- Wordle --------------------");
            Console.WriteLine("Guesses:");
            for (int i = 0; i < WordleState.MAX_GUESSES; ++i)
            {
                Console.Write("- ");
                PrintGuess(i);
                Console.WriteLine();
            }
            Console.Write($"Guessed Letters: ");
            PrintGuessedLetters();
            Console.WriteLine("\n------------------------------------------------");
            Console.SetCursorPosition(left, top);
            Console.ForegroundColor = colour;
        }
        private void UpdateBoard()
        {
            ConsoleColor colour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            (int left, int top) = Console.GetCursorPosition();
            Console.SetCursorPosition(0, m_State.GuessCount + 1);
            Console.Write("- ");
            PrintGuess(m_State.GuessCount - 1);
            Console.SetCursorPosition(0, WordleState.MAX_GUESSES + 2);
            Console.Write("Guessed Letters: ");
            PrintGuessedLetters();
            Console.SetCursorPosition(left, top);
            Console.ForegroundColor = colour;
        }
        private void PrintGuess(int guess_index)
        {
            ConsoleColor colour = Console.ForegroundColor;
            for (int i = 0; i < WordleState.WORD_LENGTH; ++i)
            {
                if (m_State.Greens[guess_index][i]) Console.ForegroundColor = ConsoleColor.Green;
                else if (m_State.Yellows[guess_index][i]) Console.ForegroundColor = ConsoleColor.Yellow;
                else Console.ForegroundColor = colour;
                Console.Write(m_State.Guesses[guess_index][i]);
            }
            Console.ForegroundColor = colour;
        }
        private void PrintGuessedLetters()
        {
            foreach ((char c, WordleState.LetterGuess letter_guess) in m_State.GuessedLetters)
            {
                Console.ForegroundColor = letter_guess switch
                {
                    WordleState.LetterGuess.Unknown => ConsoleColor.White,
                    WordleState.LetterGuess.IsIn => ConsoleColor.Yellow,
                    WordleState.LetterGuess.IsNotIn => ConsoleColor.DarkGray,
                    _ => ConsoleColor.White
                };
                Console.Write(c);
            }
        }

        private WordleState m_State;
        private readonly int m_BoxHeight;
    }
}
