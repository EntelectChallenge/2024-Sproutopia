using System;


string userInput = "";
bool connectToEngine = false;

do
{
    Console.Write("Do you want to connect to game engine (Y/N)? ");
    string input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input.Length != 1)
    {
        Console.WriteLine("Invalid input. Please enter either 'Y' or 'N'.");
        continue;
    }

    userInput = input[0].ToString().ToUpper(); 

    if (userInput != "Y" && userInput != "N")
    {
        Console.WriteLine("Invalid input. Please enter either 'Y' or 'N'.");
    }

} while (userInput != "Y" && userInput != "N");

if (userInput == "Y")
{
    connectToEngine = true;
}

using var game = new Visualiser.Game1(connectToEngine);
game.Run();