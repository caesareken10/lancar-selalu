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
    static void SetupConsole()
    {
        System.Diagnostics.Process.Start("cmd", "/c mode con: cols=80 lines=30").WaitForExit(); //opens a console 80x30, waits for program to finish
    }
    static void Main()
    {
        SetupConsole();
        Console.WriteLine(@"                              ####   ###  ####   ###  ####  ##### #     #     #   # #   #");
        Console.WriteLine(@"                              #   # #   # #   # #   # #   # #     #     #     #   # ## ##");
        Console.WriteLine(@"                              ####  ##### ####  ##### ####  ####  #     #     #   # # # #");
        Console.WriteLine(@"                              #     #   # #  #  #   # #   # #     #     #     #   # #   #");
        Console.WriteLine(@"                              #     #   # #   # #   # ####  ##### ##### #####  ###  #   #");
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
        Console.WriteLine(@"                                      +------------------------------------+");
        Console.WriteLine(@"                                      |                                    |");
        Console.WriteLine(@"                                      |       Press Enter to Continue      |");
        Console.WriteLine(@"                                      |                                    |");
        Console.WriteLine(@"                                      +------------------------------------+");
        Console.ReadLine();
    }
}