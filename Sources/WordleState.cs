using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;

namespace Wordle_CSharp.Sources
{
    internal class WordleState
    {
        public enum GuessStatus { Success, WrongLength, Formatting, NotInList, MaxGuesses, Failure, Null };
        public static string[] GUESS_MESSAGES = {
            "Success!",
            "Guesses must contain 5 letters.",
            "Guesses must be lowercase letters.",
            "Guess not in list.",
            "Reached Max Allowable Guesses."
        };
        public enum LetterGuess { IsIn, IsNotIn, Unknown };

        public const int WORD_LENGTH = 5;
        public const int MAX_GUESSES = 6;

        public static string[] WORDS = Array.Empty<string>();

        public static string GetSolutionDir()
        {
            string cwd = Directory.GetCurrentDirectory();
            DirectoryInfo? current = new(cwd);

            while (true)
            {
                if (Directory.GetFiles(current.FullName).Any(s => s.Contains(".sln"))) return current.FullName;
                current = current?.Parent;
                if (current == null) return string.Empty;
            }
        }
        public static void ReadWords()
        {
            bool is_application = false;
            string raw_words = string.Empty;

            try
            {
                raw_words = File.ReadAllText("Resources\\Words.txt");
                is_application = true;
            }
            catch (FileNotFoundException) { /* Normal. Means it's not an application and it's in development */ }
            catch (DirectoryNotFoundException) { /* Normal. Means it's not an application and it's in development */ }
            catch (Exception exception)
            {
                Console.WriteLine($"Unhandled Exception: {exception.Message}");
                return;
            }
            if (!is_application)
            {
                Console.WriteLine($"{GetSolutionDir()}\\Resources\\Words.txt");
                try { raw_words = File.ReadAllText($"{GetSolutionDir()}\\Resources\\Words.txt"); }
                catch (FileNotFoundException exception)
                {
                    Console.WriteLine($"Failed to Locate Words.txt File: {exception.Message}");
                    return;
                }
                catch (DirectoryNotFoundException exception)
                {
                    Console.WriteLine($"Failed to Locate Words.txt File: {exception.Message}");
                    return;
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Unhandled Exception: {exception.Message}");
                    return;
                }
            }
            WORDS = raw_words.
                Where(c => LOWERCASE.Contains(c)).
                Select((c, i) => new { c, i }).
                GroupBy(v => v.i / WORD_LENGTH, v => v.c).
                Cast<IEnumerable<char>>().
                Select(v => string.Concat(v)).
                ToArray();
        }

        public static readonly char[] LOWERCASE = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

        public WordleState()
        {
            DefaultInitialize();
        }
        public WordleState(string file_path)
        {
            if (!File.Exists(file_path))
            {
                DefaultInitialize();
                return;
            }
            string raw_json_data;
            try { raw_json_data = File.ReadAllText(file_path); }
            catch (Exception exception)
            {
                Console.WriteLine($"Failed to Read Save State. Unhandled exception: {exception.Message}");
                return;
            }
            SaveState? save_state = Newtonsoft.Json.JsonConvert.DeserializeObject<SaveState>(raw_json_data);
            if (save_state == null)
            {
                DefaultInitialize();
                return;
            }
            m_Word = save_state.Word;
            m_Guesses = save_state.Guesses;
            m_GuessCount = save_state.GuessCount;
            m_GuessedLetters = new(LOWERCASE.Select(c => KeyValuePair.Create(c, m_Guesses.Any(guess => guess.Contains(c)) ? m_Word.Contains(c) ? LetterGuess.IsIn : LetterGuess.IsNotIn : LetterGuess.Unknown)));
            m_Yellows = new bool[MAX_GUESSES][];
            m_Greens = new bool[MAX_GUESSES][];
            for (int i = 0; i < MAX_GUESSES; ++i)
            {
                m_Yellows[i] = new bool[WORD_LENGTH];
                m_Greens[i] = new bool[WORD_LENGTH];
            }
            for (int i = 0; i < m_GuessCount; ++i)
            {
                for (int j = 0; j < WORD_LENGTH; ++j)
                {
                    m_Yellows[i][j] = m_Word.Contains(m_Guesses[i][j]);
                    m_Greens[i][j] = m_Word[j] == m_Guesses[i][j];
                }
            }
        }

        private void DefaultInitialize()
        {
            m_Word = WORDS[new Random().Next(WORDS.Length)];
            m_Guesses = new string[MAX_GUESSES];
            for (int i = 0; i < MAX_GUESSES; i++) { m_Guesses[i] = "     "; }
            m_GuessCount = 0;
            m_GuessedLetters = new(LOWERCASE.Select(c => KeyValuePair.Create(c, LetterGuess.Unknown)));
            m_Yellows = new bool[MAX_GUESSES][].Select(arr => new bool[WORD_LENGTH]).ToArray();
            m_Greens = new bool[MAX_GUESSES][].Select(arr => new bool[WORD_LENGTH]).ToArray();
        }

        public GuessStatus Guess(string word)
        {
            if (word.Any(c => !char.IsAsciiLetterLower(c))) return GuessStatus.Formatting;
            if (word.Length != WORD_LENGTH) return GuessStatus.WrongLength;
            if (!WORDS.Contains(word)) return GuessStatus.NotInList;
            if (m_Word == word) return GuessStatus.Success;
            for (int i = 0; i < WORD_LENGTH; ++i)
            {
                if (m_GuessedLetters[word[i]] == LetterGuess.Unknown) m_GuessedLetters[word[i]] = m_Word.Contains(word[i]) ? LetterGuess.IsIn : LetterGuess.IsNotIn;
                m_Yellows[m_GuessCount][i] = m_Word.Contains(word[i]);
                m_Greens[m_GuessCount][i] = m_Word[i] == word[i];
            }
            m_Guesses[m_GuessCount++] = word;
            if (m_GuessCount == MAX_GUESSES) return GuessStatus.MaxGuesses;
            return GuessStatus.Failure;
        }

        public void SaveToFile(string file_path)
        {
            string raw_json_data = Newtonsoft.Json.JsonConvert.SerializeObject(new SaveState(m_Word, m_Guesses, m_GuessCount), Newtonsoft.Json.Formatting.Indented);
            FileStream file_stream;
            try { file_stream = File.Open(file_path, FileMode.Create, FileAccess.Write); }
            catch (Exception exception)
            {
                Console.WriteLine($"Unable to Open Save State for Write. Unhandled exception: {exception.Message}");
                return;
            }
            try { file_stream.Write(Encoding.UTF8.GetBytes(raw_json_data), 0, raw_json_data.Length); }
            catch (Exception exception)
            {
                Console.WriteLine($"Unable to Write to Save State. Unhandled exception: {exception.Message}");
                return;
            }
            file_stream.Flush();
        }

        public string Word { get => m_Word; }
        public string[] Guesses { get => m_Guesses; }
        public int GuessCount { get => m_GuessCount; }
        public Dictionary<char, LetterGuess> GuessedLetters { get => m_GuessedLetters; }
        public bool[][] Yellows { get => m_Yellows; }
        public bool[][] Greens { get => m_Greens; }

        private string m_Word;
        private string[] m_Guesses;
        private int m_GuessCount = 0;
        private Dictionary<char, LetterGuess> m_GuessedLetters;
        private bool[][] m_Yellows;
        private bool[][] m_Greens;
    }
}
