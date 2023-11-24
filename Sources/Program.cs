namespace Wordle_CSharp.Sources
{
    internal class Program
    {
        static void Main()
        {
            WordleState.ReadWords();
            Wordle game = new();
            while (game.Play() != Wordle.GameStatus.Quit)
            {
                Console.Write("Continue (y/n): ");
                if ((Console.ReadLine() ?? "") == "n")
                {
                    File.Delete("save_state.json");
                    break;
                }
                Console.Clear();
            }
            Console.Clear();
        }
    }
}