using System;
using System.Diagnostics;
using System.Threading;

class TextMazeGame
{
    private char[,] maze;
    private int playerX, playerY;
    private int startX, startY;
    private int endX, endY;
    private int wallHits;
    private readonly int maxWallHits = 3;
    private Stopwatch gameTimer;
    private readonly TimeSpan timeLimit = TimeSpan.FromMinutes(3); // 3分鐘時間限制

    public TextMazeGame()
    {
        // 初始化迷宮 (可以替換成您想要的迷宮設計)
        maze = new char[,]
        {
            {'#', '#', '#', '#', '#', '#', '#', '#', '#', '#'},
            {'#', 'S', ' ', ' ', '#', ' ', ' ', ' ', ' ', '#'},
            {'#', '#', '#', ' ', '#', ' ', '#', '#', ' ', '#'},
            {'#', ' ', '#', ' ', ' ', ' ', ' ', '#', ' ', '#'},
            {'#', ' ', '#', '#', '#', '#', ' ', '#', ' ', '#'},
            {'#', ' ', ' ', ' ', ' ', '#', ' ', '#', ' ', '#'},
            {'#', '#', '#', '#', ' ', '#', ' ', '#', ' ', '#'},
            {'#', ' ', ' ', ' ', ' ', ' ', ' ', '#', ' ', '#'},
            {'#', ' ', '#', '#', '#', '#', '#', '#', 'E', '#'},
            {'#', '#', '#', '#', '#', '#', '#', '#', '#', '#'}
        };

        // 找到起點和終點
        FindStartAndEnd();

        // 初始化玩家位置
        playerX = startX;
        playerY = startY;
        maze[playerY, playerX] = 'P';

        wallHits = 0;
        gameTimer = new Stopwatch();
    }

    private void FindStartAndEnd()
    {
        for (int y = 0; y < maze.GetLength(0); y++)
        {
            for (int x = 0; x < maze.GetLength(1); x++)
            {
                if (maze[y, x] == 'S')
                {
                    startX = x;
                    startY = y;
                }
                else if (maze[y, x] == 'E')
                {
                    endX = x;
                    endY = y;
                }
            }
        }
    }

    public void StartGame()
    {
        Console.WriteLine("歡迎來到文字迷宮遊戲！");
        Console.WriteLine("使用 W/A/S/D 或方向鍵移動，P是你的位置");
        Console.WriteLine("# 是牆壁，E是終點");
        Console.WriteLine($"你最多只能撞牆 {maxWallHits} 次，時間限制 {timeLimit.TotalMinutes} 分鐘");
        Console.WriteLine("按任意鍵開始遊戲...");
        Console.ReadKey(true);

        gameTimer.Start();
        PlayGame();
    }

    private void PlayGame()
    {
        bool gameOver = false;
        bool win = false;

        while (!gameOver)
        {
            Console.Clear();
            PrintMaze();
            PrintStatus();

            if (CheckWinCondition())
            {
                gameOver = true;
                win = true;
                break;
            }

            if (CheckLoseConditions())
            {
                gameOver = true;
                win = false;
                break;
            }

            var input = GetInput();
            ProcessInput(input);
        }

        gameTimer.Stop();
        Console.Clear();

        if (win)
        {
            Console.WriteLine("恭喜你贏了！");
            Console.WriteLine($"所用時間: {gameTimer.Elapsed:mm\\:ss}");
            Console.WriteLine($"撞牆次數: {wallHits}/{maxWallHits}");
        }
        else
        {
            Console.WriteLine("遊戲結束！你輸了！");

            if (wallHits >= maxWallHits)
                Console.WriteLine($"撞牆次數超過限制 ({wallHits}/{maxWallHits})");
            else
                Console.WriteLine($"時間用完 ({gameTimer.Elapsed:mm\\:ss}/{timeLimit:mm\\:ss})");
        }

        Console.WriteLine("按任意鍵退出...");
        Console.ReadKey(true);
    }

    private void PrintMaze()
    {
        for (int y = 0; y < maze.GetLength(0); y++)
        {
            for (int x = 0; x < maze.GetLength(1); x++)
            {
                Console.Write(maze[y, x]);
                Console.Write(' ');
            }
            Console.WriteLine();
        }
    }

    private void PrintStatus()
    {
        Console.WriteLine();
        Console.WriteLine($"當前位置: ({playerX}, {playerY})");
        Console.WriteLine($"撞牆次數: {wallHits}/{maxWallHits}");
        Console.WriteLine($"剩餘時間: {(timeLimit - gameTimer.Elapsed).ToString("mm\\:ss")}");
    }

    private ConsoleKey GetInput()
    {
        while (true)
        {
            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.W || key == ConsoleKey.UpArrow ||
                key == ConsoleKey.A || key == ConsoleKey.LeftArrow ||
                key == ConsoleKey.S || key == ConsoleKey.DownArrow ||
                key == ConsoleKey.D || key == ConsoleKey.RightArrow)
            {
                return key;
            }
        }
    }

    private void ProcessInput(ConsoleKey input)
    {
        int newX = playerX;
        int newY = playerY;

        switch (input)
        {
            case ConsoleKey.W:
            case ConsoleKey.UpArrow:
                newY--;
                break;
            case ConsoleKey.A:
            case ConsoleKey.LeftArrow:
                newX--;
                break;
            case ConsoleKey.S:
            case ConsoleKey.DownArrow:
                newY++;
                break;
            case ConsoleKey.D:
            case ConsoleKey.RightArrow:
                newX++;
                break;
        }

        // 檢查移動是否有效
        if (newX >= 0 && newX < maze.GetLength(1) &&
            newY >= 0 && newY < maze.GetLength(0))
        {
            if (maze[newY, newX] == '#' || maze[newY, newX] == 'S')
            {
                wallHits++;
                Console.Beep(300, 200); // 撞牆音效
                Thread.Sleep(200); // 暫停一下讓玩家知道撞牆了
            }
            else
            {
                // 移動玩家
                maze[playerY, playerX] = ' '; // 清除舊位置
                playerX = newX;
                playerY = newY;
                maze[playerY, playerX] = 'P'; // 設置新位置
            }
        }
        else
        {
            wallHits++;
            Console.Beep(300, 200);
            Thread.Sleep(200);
        }
    }

    private bool CheckWinCondition()
    {
        return playerX == endX && playerY == endY;
    }

    private bool CheckLoseConditions()
    {
        return wallHits >= maxWallHits || gameTimer.Elapsed >= timeLimit;
    }

    static void Main(string[] args)
    {
        TextMazeGame game = new TextMazeGame();
        game.StartGame();
    }
}