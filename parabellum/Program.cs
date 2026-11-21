using System;
using System.IO;
using System.Threading;

class Parabellum
{

    const string FileEncounters = "encounters.txt";
    const string FileLeaderboard = "leaderboard.txt";
    const string CreditsLine = "ASCII resources: asciiart.eu (free to use with credit)";

    const int ScreenWidth = 80;
    const int TopHeight = 20;
    const int BottomHeight = 8;
    const int ScreenHeight = TopHeight + 1 + BottomHeight;
    const int SpriteRows = 8;
    const int MaxBarWidth = 50;
    const int PlayerBarWidth = 30;
    const int LogLines = 3;
    const int MaxWrapLines = 6;
    const int BoxWidth = 24;
    const int TypeDelayMs = 12;

    const int PlayerStartHp = 20;
    const int MinMaxHp = 5;
    const int MinNameLength = 3;
    const int MaxNameLength = 12;
    const int EncountersToBoss = 6;
    const int ChoicesPerRound = 3;
    const int MinPoolSize = EncountersToBoss + ChoicesPerRound;
    const int MaxPool = 30;
    const int BossHp = 50;
    const int BossIndex = 8;
    const int RestHealAmount = 8;
    const int GuardBlockPercent = 75;
    const int StartEnergy = 1;

    const int ClassWarrior = 1;
    const int ClassMage = 2;

    const int TypeReward = 0;
    const int TypeTrap = 1;
    const int TypeSpecial = 2;
    const int TypeCombat = 3;

    const int ItemPotion = 0;
    const int ItemBomb = 1;
    const int ItemPact = 2;
    const int ItemDie = 3;
    const int ItemCount = 4;

    const int ModeCore = 0;
    const int ModeWarriorMoves = 1;
    const int ModeMageCast = 2;
    const int ModeMessage = 3;

    const int ResultDeclined = 0;
    const int ResultDone = 1;
    const int ResultDied = 2;

    const int EnemyKinds = 9;
    const int PatternLength = 4;

    const int SlashDamage = 4;
    const int OverpowerDamage = 10;
    const int ExecutionerDamage = 20;
    const int RoarMultiplier = 2;
    const int ShieldReduce = 2;
    const int ShieldTurns = 3;

    const int SpellFireball = 0;
    const int SpellPoison = 1;
    const int SpellWard = 2;
    const int SpellFrostbite = 3;
    const int SpellTempest = 4;
    const int FireballDamage = 5;
    const int FrostbiteDamage = 4;
    const int FrostbiteWeaken = 3;
    const int WardReduce = 3;
    const int WardTurns = 2;
    const int PoisonStartDamage = 3;
    const int PoisonTicks = 3;
    const int TempestDamage = 26;
    const int TempestUnlockMoves = 4;
    const int TempestBacklash = 5;

    const int PotionHeal = 8;
    const int BombDamage = 6;
    const int PactMultiplier = 5;
    const int DieSides = 6;
    const int DieBackfire = 4;

    const int ScorePerEncounter = 100;
    const int ScorePerHp = 25;
    const int ScoreVictoryBonus = 1000;
    const int ScoreTurnPenalty = 5;
    const int MaxScores = 200;
    const int TopScoresShown = 10;

    static Random rng = new Random();
    static int lineCount = 0;
    static bool typingSkipped = false;

    static string playerName = "";
    static int playerClass = ClassWarrior;
    static int playerMaxHp = PlayerStartHp;
    static int playerHp = PlayerStartHp;
    static int damageBonus = 0;
    static int[] inventory = new int[ItemCount];

    static int encountersDone = 0;
    static int combatsWon = 0;
    static int totalTurns = 0;

    static string[] runLogName = new string[EncountersToBoss + 1];
    static int[] runLogDamage = new int[EncountersToBoss + 1];
    static int runLogCount = 0;

    static int[] encType = new int[MaxPool];
    static string[] encName = new string[MaxPool];
    static string[] encDesc = new string[MaxPool];
    static int[] encHp = new int[MaxPool];
    static int[] encSprite = new int[MaxPool];
    static int[] encMaxHp = new int[MaxPool];
    static int[] encDmg = new int[MaxPool];
    static int[] encDrop = new int[MaxPool];
    static bool[] encUsed = new bool[MaxPool];
    static int poolCount = 0;

    static string enemyName = "";
    static int enemyHp = 0;
    static int enemyMaxHp = 0;
    static int enemyKind = 0;
    static int patternStep = 0;
    static int turn = 1;
    static int energy = StartEnergy;
    static int movesUsed = 0;
    static int nextMultiplier = 1;
    static int flatReduce = 0;
    static int flatReduceTurns = 0;
    static bool guarding = false;
    static int enemyWeaken = 0;
    static int poisonTicks = 0;
    static int poisonDamage = 0;
    static bool pactArmed = false;
    static bool pactDoomed = false;
    static int fightDamage = 0;
    static string[] combatLog = { "", "", "" };

    static string[] lbName = new string[MaxScores];
    static string[] lbClass = new string[MaxScores];
    static int[] lbScore = new int[MaxScores];
    static string[] lbResult = new string[MaxScores];
    static int[] lbOrder = new int[MaxScores];
    static int lbCount = 0;

    static string[] itemName = { "Health Potion", "Bomb Flask", "Berserker Pact", "Fate Die" };
    static string[] itemDesc =
    {
        "Restore " + PotionHeal + " HP.",
        "Deal " + BombDamage + " damage to the enemy.",
        "Next attack x" + PactMultiplier + ", but you die the turn after!",
        "Roll a die: a random permanent buff OR debuff."
    };

    static string[] warriorName = { "", "Slash", "Overpowering Blow", "Roar", "Raise Shield", "Executioner's Stance" };
    static int[] warriorCost = { 0, 0, 2, 1, 1, 4 };
    static string[] warriorInfo =
    {
        "",
        "Basic strike, " + SlashDamage + " dmg",
        "Strong strike, " + OverpowerDamage + " dmg",
        "Next attack x" + RoarMultiplier,
        "Block " + ShieldReduce + " dmg for " + ShieldTurns + " turns",
        "ULTIMATE, " + ExecutionerDamage + " dmg"
    };

    static string[] spellName = { "Fireball", "Poison Spray", "Warding Call", "Frostbite", "Arcane Tempest" };
    static string[] spellIncantation =
    {
        "Ignis Sphaera",
        "Maledictio Tabi",
        "Praesidium Sanguinis",
        "Gelidus Rigor",
        "Lux Et Umbra In Chaos"
    };

    static int[,] enemyPattern = new int[EnemyKinds, PatternLength]
    {
        { 2, 3, 2, 5 },   
        { 3, 3, 4, 2 },   
        { 2, 4, 2, 6 },   
        { 3, 5, 2, 4 },   
        { 4, 2, 4, 3 },   
        { 1, 7, 2, 6 },   
        { 3, 4, 5, 2 },   
        { 0, 8, 2, 5 },   
        { 4, 6, 3, 9 }    
    };

    static string[][] sprites =
    {
        new string[] {
            @"    ,   ,",
            @"   /|\_/|\",
            @"  / (o o) \",
            @" |   \_/   |",
            @"  \  /|\  /",
            @"   \/ | \/",
            @"     / \",
            @"    d   b" },
        new string[] {
            @"      .-.",
            @"     (o.o)    |",
            @"      |=|     |",
            @"    __|=|__   |",
            @"   /  |=|  \  |",
            @"  /_/ |=| \_\ +",
            @"      / \",
            @"     /   \" },
        new string[] {
            @"    /\   /\",
            @"   /  \_/  \",
            @"  /  (o o)  \",
            @" /__ /|||\ __\",
            @"   // ||| \\",
            @"  //  |||  \\",
            @" //   / \   \\",
            @"/    /   \    \" },
        new string[] {
            @"     ___",
            @"    /___\",
            @"   | o o |",
            @"   |  _  |",
            @"  /|_____|\",
            @" / |  $  | \",
            @"/  |_____|  \",
            @"   /|   |\" },
        new string[] {
            @"   /\_/\",
            @"  / o o \____",
            @" (  =^=      \",
            @"  \ ______    )",
            @"  / /    \ \_/",
            @" / /      \ \",
            @"(_/        \_)" },
        new string[] {
            @"     _______",
            @"    /       \",
            @"   |  O   O  |",
            @"   |    ^    |",
            @"  /|  \___/  |\",
            @" /_|_________|_\",
            @"   |  |   |  |",
            @"  _|__|   |__|_" },
        new string[] {
            @"      .---.",
            @"     / o o \",
            @"    |   O   |",
            @"    |  ___  |",
            @"   /|       |\",
            @"  / |       | \",
            @" ~  ~~     ~~  ~",
            @"  ~    ~~~    ~" },
        new string[] {
            @"    _________",
            @"   |  _   _  |",
            @"   | (_) (_) |",
            @"   |_________|",
            @"  _|  |   |  |_",
            @" |_|  |___|  |_|",
            @"   |  |   |  |",
            @"   |__|   |__|" },
        new string[] {
            @"   |\_     _/|",
            @"   | \_ _/ | |",
            @"   \  (X X)  /",
            @"    | \___/ |",
            @"  __|/|||||\|__",
            @" /  |/ ||| \|  \",
            @"/___/  |||  \___\",
            @"      /___\" },
        new string[] {
            @"      .---.",
            @"     /     \",
            @"    |  ^ ^  |",
            @"     \  v  /",
            @"    /|`---'|\",
            @"   / |     | \",
            @"  /__|_____|__\" },
        new string[] {
            @"      .---.",
            @"     /     \",
            @"    | - -   |",
            @"     \ ___ /",
            @"   __/|   |\__",
            @"  /   | $ |   \",
            @" /____|___|____\" },
        new string[] {
            @"        *",
            @"       /|\",
            @"      / | \",
            @"    _/__|__\_",
            @"   |  .-*-.  |",
            @"   |__|___|__|",
            @"   /_________\" }
    };

    static string[] skullArt =
    {
        @"     _______",
        @"    /       \",
        @"   |  O   O  |",
        @"   |    ^    |",
        @"    \  |||  /",
        @"     |_____|"
    };

    static string[] warriorFrame =
    {
        @"     \  |  /",
        @"   -- (   ) --",
        @"     /  |  \",
        @"  ______|______",
        @"        o  /",
        @"       /|\/",
        @"       / \",
        @"  _____/_\______"
    };

    static string[] mageFrame =
    {
        @"   *    .   *     .",
        @" .    *   .    *",
        @"      /\   *",
        @"     /  \",
        @"    ( oo )  /",
        @"    /|  |\ /",
        @"   / |__|/*",
        @"      /\"
    };

    static string[][] encounterDialogue =
    {
        new string[] {
            "Smith: Traveler! That blade of yours is duller than a butter knife. Let me fix that.",
            "He works the whetstone for a while. The edge sings. You feel sharper already." },
        new string[] {
            "Healer: Rest, child. The road is cruel to those who walk it alone.",
            "Warm light settles over you. Your wounds ease and your spirit steadies." },
        new string[] {
            "Knight: Forty winters I fought. Let me teach you what the bards forget.",
            "He drills you until your arms ache. You leave tougher and wiser." },
        new string[] {
            "Merchant: Friend! Take a sample, on the house, no strings attached!",
            "You taste something sweet. Too late, you notice his smile has no warmth.",
            "Merchant: A pleasure doing business!" },
        new string[] {
            "Beggar: Spare a coin for a cursed man?",
            "You shake him off, but his fingers leave a cold mark on your weapon arm." },
        new string[] {
            "The altar speaks in a voice like grinding stone: 'Power for a price.'",
            "A vial of black blood waits in a carved hollow. You take it." },
        new string[] {
            "A bone die rests in the shrine, carved with strange symbols.",
            "Something whispers that the gods love a gambler. You pocket the die." },
        new string[] {
            "Goblin: Shiny! Shiny! Give shiny to Grik!",
            "The goblin lunges from the bushes!" },
        new string[] {
            "An empty helmet turns toward you. The bones inside rattle to life.",
            "Bone Soldier: ...defend... the... kingdom..." },
        new string[] {
            "Something drops from above and skitters across your boots.",
            "Eight eyes catch the light. The spider hisses." },
        new string[] {
            "Bandit: Your coin or your life. Mostly the coin.",
            "He draws a notched blade and grins." },
        new string[] {
            "A low growl rolls through the trees.",
            "The dire wolf bares its teeth and circles you." },
        new string[] {
            "The troll roars and shakes dust from the cave ceiling.",
            "Troll: Little human make good snack!" },
        new string[] {
            "The cold deepens. A pale shape drifts out of the mist.",
            "Wraith: Join us... join us..." },
        new string[] {
            "The golem's eyes flare with amber light. It rises with a grinding groan.",
            "Stone Golem: INTRUDER. DESTROY." }
    };

    static string[] bossIntro =
    {
        "The gates of the Black Keep groan open. Torches flare along a hall of bones.",
        "Warlord Malachar: So the little hero finally arrives. I have waited long.",
        "Malachar: Parabellum, they say. Prepare for war. Then prepare to die!"
    };

    static string[] warriorEndingLines =
    {
        "Malachar crashes to the ground. The keep falls silent.",
        "You plant your blade in the stone and raise your fist to the dawn.",
        "The war is over. But the songs of the warrior who prepared for it will last forever."
    };

    static string[] mageEndingLines =
    {
        "The last of the arcane storm fades. Malachar is only ash on the wind.",
        "Your voice still echoes in the chamber, every word perfectly spoken.",
        "The war is over. Scholars will study your incantations for a hundred years."
    };

    static string[] tutorialRun =
    {
        "Survive " + EncountersToBoss + " encounters, then face the final boss.",
        "Each round you are shown " + ChoicesPerRound + " paths. Pick one.",
        "Read the descriptions: judge the outcome of your choice yourself!",
        "Paths can be fights, kind strangers, traps or strange items.",
        "An encounter never repeats within one run.",
        "You start with " + PlayerStartHp + " HP. Death is PERMANENT: you restart from zero.",
        "You always act first in a fight.",
        "Winning a fight heals you for " + RestHealAmount + " HP."
    };

    static string[] tutorialWarrior =
    {
        "You gain 1 energy at the end of every turn (you start with " + StartEnergy + ").",
        "Stronger moves cost more energy. Slash is free.",
        "",
        "[1] Slash                 0 EN   " + SlashDamage + " damage",
        "[2] Overpowering Blow     2 EN   " + OverpowerDamage + " damage",
        "[3] Roar                  1 EN   your next attack x" + RoarMultiplier,
        "[4] Raise Shield          1 EN   -" + ShieldReduce + " damage taken, " + ShieldTurns + " turns",
        "[5] Executioner's Stance  4 EN   " + ExecutionerDamage + " damage"
    };

    static string[] tutorialMage =
    {
        "No energy, but you must TYPE each incantation exactly.",
        "Capitalization counts. A typo wastes your whole turn!",
        "Press Enter on an empty line to cancel without losing the turn.",
        "",
        "Arcane Tempest unlocks after " + TempestUnlockMoves + " successful spells.",
        "Mistype the Tempest and it hits YOU for " + TempestBacklash + " damage.",
        "Poison Spray burns the enemy over several turns."
    };

    static string[] tutorialItems =
    {
        "[1] Attack / Cast Spell   [2] Items   [3] Guard",
        "",
        "Guard blocks " + GuardBlockPercent + "% of the enemy's next attack.",
        "Items are FREE: you can drink a potion and still attack.",
        "You start with 1 Health Potion. Inventory space is unlimited.",
        "The enemy shows what it will do next, plan around it!"
    };

    static void SetupConsole()
    {
        // The VS Code launch configuration opens the external terminal.
        // This method only attempts to resize that terminal to the game's layout.
        try
        {
            if (!Console.IsOutputRedirected)
            {
                Console.SetWindowSize(ScreenWidth, 30);
            }
        }
        catch
        {
            // Some terminals do not allow programmatic resizing.
            // The game can still run using the terminal's current dimensions.
        }
    }

    static void ShowIntroScreen()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(@"                              ####   ###  ####   ###  ####  ##### #     #     #   # #   #");
        Console.WriteLine(@"                              #   # #   # #   # #   # #   # #     #     #     #   # ## ##");
        Console.WriteLine(@"                              ####  ##### ####  ##### ####  ####  #     #     #   # # # #");
        Console.WriteLine(@"                              #     #   # #  #  #   # #   # #     #     #     #   # #   #");
        Console.WriteLine(@"                              #     #   # #   # #   # ####  ##### ##### #####  ###  #   #");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine(@"                                                          |>");
        Console.WriteLine(@"                                                          A");
        Console.WriteLine(@"                                                         | |");
        Console.WriteLine(@"                                                        |   |");
        Console.WriteLine(@"          /\                         <|                |     |                    |>             /\");
        Console.WriteLine(@"         /  \                        /\               |       |                  /\             /  \");
        Console.WriteLine(@"        /~~~~\       /\             /  \      |>     |         |         <|     /  \           /~~~~\");
        Console.WriteLine(@"       /      \     /  \           /    \     |_____|___________|_________|    /    \         /      \");
        Console.WriteLine(@"      /        \   /~~~~\         /______\___|_|__|  |__|  |__|  |__|  |_|_|__/______\       /        \");
        Console.WriteLine(@"     /          \ /      \        | ___  |  |  ||  |  ||  |  ||  |  ||  |  |  | ___ |       /          \    /\");
        Console.WriteLine(@"    /            \        \       ||   | |__|__||__|__||__|__||__|__||__|__|__||   ||      /            \  /~~\");
        Console.WriteLine(@"   /              \        \      ||___|   |   /\   |  |   |   |   |   |   |  ||   ||     /              \/    \");
        Console.WriteLine(@"  /                \        \ ___ ||___|___|__/__\__|__|___|___|___|___|___|__||___||____/                \     \");
        Console.WriteLine(@" /                  \       |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |                  \     \");
        Console.WriteLine(@"/                    \      |__|__|__|__|__|__|__|__|__|__|__|__|__|__|__|__|__|__|__|__|                   \     \");
        Console.WriteLine(@"_________________________/_________________________________________________________________\____________________________");
        Console.WriteLine(@"                         _/~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\_");
        Console.WriteLine(@"                         ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"                                      +------------------------------------+");
        Console.WriteLine(@"                                      |                                    |");
        Console.WriteLine(@"                                      |       Press Enter to Continue      |");
        Console.WriteLine(@"                                      |                                    |");
        Console.WriteLine(@"                                      +------------------------------------+");
        Console.ResetColor();
        Console.ReadLine();
    }

    static void Main()
    {
        Console.Title = "PARABELLUM";
        SetupConsole();
        ShowIntroScreen();

        PrepareSampleFiles();
        poolCount = EncounterPool();
        if (poolCount < MinPoolSize)
        {
            Console.WriteLine($"{FileEncounters} needs at least {MinPoolSize} valid lines, found {poolCount}.");
            return;
        }

        bool running = true;
        while (running)
        {
            ShowTitleScreen();
            Console.WriteLine();
            OutLine(Center("[1] New Game       [2] Leaderboard       [3] Quit"), ConsoleColor.White);
            Console.WriteLine();
            int choice = GetPlayerInput("  > ", 1, 3);

            switch (choice)
            {
                case 1:
                    PlayRun();
                    break;
                case 2:
                    LeaderboardMenu();
                    break;
                case 3:
                    running = false;
                    Console.WriteLine("  Farewell, traveler.");
                    break;
            }
        }
    }

    static void PlayRun()
    {
        LoadPlayer();
        bool won = RunGameLoop();
        if (won)
        {
            if (playerClass == ClassWarrior) { WarriorEnding(); }
            else { MageEnding(); }
        }
        else
        {
            string[] deathLines = { "Your journey ends here. Deaths are permanent, the road begins anew." };
            DisplayDialogue(deathLines, "YOU HAVE FALLEN", skullArt, -1, 0);
        }
        FinishRun(won);
    }

    static void ClearScreen()
    {
        Console.Clear();
        lineCount = 0;
    }

    static void Out(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    static void Nl()
    {
        Console.WriteLine();
        lineCount = lineCount + 1;
    }

    static void OutLine(string text, ConsoleColor color)
    {
        Out(text, color);
        Nl();
    }

    static void PadTo(int rows)
    {
        while (lineCount < rows) { Nl(); }
    }

    static string Spaces(int count)
    {
        if (count <= 0) { return ""; }
        return new string(' ', count);
    }

    static string Center(string text)
    {
        return Spaces((ScreenWidth - text.Length) / 2) + text;
    }

    static string PadCenter(string text, int width)
    {
        return Spaces((width - text.Length) / 2) + text;
    }

    static string Fit(string text, int width)
    {
        if (text.Length > width) { return text.Substring(0, width); }
        return text;
    }

    static string Signed(int value)
    {
        if (value > 0) { return "+" + value; }
        return Convert.ToString(value);
    }

    static void WaitEnter(string message)
    {
        Out(message, ConsoleColor.DarkGray);
        Console.ReadLine();
    }

    static int ToInt(string text)
    {
        int value;
        if (int.TryParse(text, out value)) { return value; }
        return 0;
    }

    static string ClassName()
    {
        if (playerClass == ClassWarrior) { return "Warrior"; }
        return "Mage";
    }

    static void TypeText(string text, int index)
    {
        if (index >= text.Length) { return; }

        Console.Write(text[index]);
        if (!typingSkipped)
        {
            if (Console.KeyAvailable)
            {
                Console.ReadKey(true);
                typingSkipped = true;
            }
            else
            {
                Thread.Sleep(TypeDelayMs);
            }
        }
        TypeText(text, index + 1);
    }

    static string[] WrapText(string text, int width)
    {
        string[] lines = new string[MaxWrapLines];
        int count = 0;
        string current = "";
        string[] words = text.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (current.Length == 0)
            {
                current = words[i];
            }
            else if (current.Length + 1 + words[i].Length <= width)
            {
                current = current + " " + words[i];
            }
            else
            {
                if (count < MaxWrapLines) { lines[count] = current; count = count + 1; }
                current = words[i];
            }
        }
        if (current.Length > 0 && count < MaxWrapLines) { lines[count] = current; count = count + 1; }
        for (int i = count; i < MaxWrapLines; i++) { lines[i] = ""; }
        return lines;
    }

    static int GetPlayerInput(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            string raw = Console.ReadLine();
            int value;
            if (int.TryParse(raw, out value) && value >= min && value <= max)
            {
                return value;
            }
            Console.WriteLine($"  Invalid input. Enter a number from {min} to {max}.");
        }
    }

    static void DrawHpBar(int hp, int maxHp, int width)
    {
        if (hp < 0) { hp = 0; }
        int filled = 0;
        int percent = 0;
        if (maxHp > 0)
        {
            filled = hp * width / maxHp;
            percent = hp * 100 / maxHp;
        }
        if (hp > 0 && filled == 0) { filled = 1; }
        if (filled > width) { filled = width; }

        ConsoleColor color = ConsoleColor.Green;
        if (percent <= 50) { color = ConsoleColor.Yellow; }
        if (percent <= 25) { color = ConsoleColor.Red; }

        Out("[", ConsoleColor.Gray);
        Out(new string('#', filled), color);
        Out(new string('-', width - filled), ConsoleColor.DarkGray);
        Out("]", ConsoleColor.Gray);
    }

    static int BarWidthFor(int maxHp, int cap)
    {
        if (maxHp > cap) { return cap; }
        if (maxHp < 1) { return 1; }
        return maxHp;
    }

    static void PrintSprite(string[] sprite)
    {
        int widest = 0;
        for (int i = 0; i < sprite.Length; i++)
        {
            if (sprite[i].Length > widest) { widest = sprite[i].Length; }
        }
        string margin = Spaces((ScreenWidth - widest) / 2);
        for (int r = 0; r < SpriteRows; r++)
        {
            if (r < sprite.Length) { OutLine(margin + sprite[r], ConsoleColor.Cyan); }
            else { Nl(); }
        }
    }

    static string BuildStatus(bool inCombat)
    {
        string s = "";
        if (inCombat)
        {
            if (playerClass == ClassWarrior) { s = s + $"Energy: {energy}  "; }
            else { s = s + $"Spells cast: {movesUsed}  "; }
            if (nextMultiplier > 1) { s = s + $"NEXT HIT x{nextMultiplier}  "; }
            if (flatReduceTurns > 0) { s = s + $"Ward -{flatReduce} ({flatReduceTurns}t)  "; }
            if (poisonTicks > 0) { s = s + $"Enemy poisoned ({poisonTicks}t)  "; }
            if (enemyWeaken > 0) { s = s + "Enemy weakened  "; }
            if (pactDoomed) { s = s + "PACT DUE!  "; }
        }
        if (damageBonus != 0) { s = s + $"Damage {Signed(damageBonus)}  "; }
        s = s + $"Potions: {inventory[ItemPotion]}";
        return s;
    }

    static void ShowTitleScreen()
    {
        ClearScreen();
        
        string[] titleArt =
        {
            @" ____  " + @"    _    " + @" ____  " + @"    _    " + @" ____  " + @" _____ " + @" _     " + @" _     " + @" _   _ " + @" __  __ ",
            @"|  _ \ " + @"   / \   " + @"|  _ \ " + @"   / \   " + @"| __ ) " + @"| ____|" + @"| |    " + @"| |    " + @"| | | |" + @"|  \/  |",
            @"| |_) |" + @"  / _ \  " + @"| |_) |" + @"  / _ \  " + @"|  _ \ " + @"|  _|  " + @"| |    " + @"| |    " + @"| | | |" + @"| |\/| |",
            @"|  __/ " + @" / ___ \ " + @"|  _ < " + @" / ___ \ " + @"| |_) |" + @"| |___ " + @"| |___ " + @"| |___ " + @"| |_| |" + @"| |  | |",
            @"|_|    " + @"/_/   \_\" + @"|_| \_\" + @"/_/   \_\" + @"|____/ " + @"|_____|" + @"|_____|" + @"|_____|" + @" \___/ " + @"|_|  |_|"
        };

        Nl(); Nl(); Nl();
        for (int i = 0; i < titleArt.Length; i++)
        {
            OutLine(Center(titleArt[i]), ConsoleColor.Red);
        }
        Nl();
        OutLine(Center("<<<====[  A Turn-Based Medieval RPG  ]====>>>"), ConsoleColor.Yellow);
        OutLine(Center("para bellum  -  prepare for war"), ConsoleColor.DarkGray);
        Nl();
        OutLine(Center("Choose a Warrior or a Mage. Every run is different."), ConsoleColor.Gray);
        OutLine(Center("Death is permanent."), ConsoleColor.Gray);
        Nl(); Nl();
        OutLine(Center(CreditsLine), ConsoleColor.DarkGray);
    }

    static void LoadPlayer()
    {
        ClearScreen();
        OutLine(Center("=== CREATE YOUR HERO ==="), ConsoleColor.Yellow);
        Nl();

        string name = "";
        bool valid = false;
        while (!valid)
        {
            Console.Write($"  Username ({MinNameLength}-{MaxNameLength} characters): ");
            name = Console.ReadLine();
            if (name == null) { name = ""; }
            name = name.Trim();

            if (name.Length < MinNameLength || name.Length > MaxNameLength)
            {
                Console.WriteLine($"  Name must be {MinNameLength} to {MaxNameLength} characters long.");
            }
            else if (name.Contains(","))
            {
                
                Console.WriteLine("  Please do not use commas in your name.");
            }
            else
            {
                valid = true;
            }
        }

        Console.WriteLine();
        Console.WriteLine("  Choose your class:");
        Console.WriteLine("    [1] WARRIOR - pick moves from a menu, energy grows every turn.");
        Console.WriteLine("    [2] MAGE    - type incantations EXACTLY. One typo and you do nothing.");
        int chosenClass = GetPlayerInput("  > ", ClassWarrior, ClassMage);

        Console.WriteLine();
        int wantTutorial = GetPlayerInput("  Play the tutorial? [1] Yes  [2] Skip: ", 1, 2);

        ResetRun(name, chosenClass);
        if (wantTutorial == 1) { RunTutorial(); }
    }

    static void ResetRun(string name, int chosenClass)
    {
        playerName = name;
        playerClass = chosenClass;
        playerMaxHp = PlayerStartHp;
        playerHp = PlayerStartHp;
        damageBonus = 0;
        for (int i = 0; i < ItemCount; i++) { inventory[i] = 0; }
        inventory[ItemPotion] = 1;
        for (int i = 0; i < poolCount; i++) { encUsed[i] = false; }
        encountersDone = 0;
        combatsWon = 0;
        totalTurns = 0;
        runLogCount = 0;
    }

    static void RunTutorial()
    {
        DrawBox("HOW A RUN WORKS", tutorialRun);
        if (playerClass == ClassWarrior) { DrawBox("WARRIOR - ENERGY AND STEEL", tutorialWarrior); }
        else { DrawBox("MAGE - SPEAK THE WORDS", tutorialMage); }
        DrawBox("ACTIONS, ITEMS AND GUARD", tutorialItems);
    }

    static void DrawBox(string title, string[] lines)
    {
        int widest = title.Length;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Length > widest) { widest = lines[i].Length; }
        }
        int width = widest + 2;
        string margin = Spaces((ScreenWidth - width - 2) / 2);
        int boxHeight = lines.Length + 4;
        int topPad = (ScreenHeight - boxHeight - 2) / 2;

        ClearScreen();
        for (int i = 0; i < topPad; i++) { Console.WriteLine(); }
        OutLine(margin + "+" + new string('-', width) + "+", ConsoleColor.Yellow);
        OutLine(margin + "|" + PadCenter(title, width).PadRight(width) + "|", ConsoleColor.Yellow);
        OutLine(margin + "+" + new string('-', width) + "+", ConsoleColor.Yellow);
        for (int i = 0; i < lines.Length; i++)
        {
            OutLine(margin + "| " + lines[i].PadRight(width - 2) + " |", ConsoleColor.White);
        }
        OutLine(margin + "+" + new string('-', width) + "+", ConsoleColor.Yellow);
        Console.WriteLine();
        WaitEnter(margin + "[Press ENTER to continue]");
    }
    
