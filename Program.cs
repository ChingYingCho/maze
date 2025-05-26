using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

class TextMazeGame
{
    private char[,] maze;                  // 迷宮的二維陣列
    private int playerX, playerY;          // 玩家目前位置
    private int startX, startY;            // 起點座標
    private int endX, endY;                // 終點座標
    private int wallHits;                  // 撞牆次數
    private readonly int maxWallHits = 3;  // 最多允許撞牆次數
    private Stopwatch gameTimer;           // 遊戲計時器
    private readonly TimeSpan timeLimit = TimeSpan.FromMinutes(1); // 時間限制 1 分鐘

    public TextMazeGame()
    {
        int width = 21;  // 迷宮寬度（奇數才能有牆與通道交錯）
        int height = 15; // 迷宮高度（奇數）
        GenerateRandomMaze(width, height); // 產生隨機迷宮

        wallHits = 0;
        gameTimer = new Stopwatch();       // 初始化計時器
    }

    // 產生隨機迷宮的方法
    private void GenerateRandomMaze(int width, int height)
    {
        maze = new char[height, width];

        // 將整個迷宮初始化為牆壁
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                maze[y, x] = '#';

        Random rand = new Random();

        // 隨機選擇一個奇數座標作為起始挖洞點
        int sx = rand.Next(width / 2) * 2 + 1;
        int sy = rand.Next(height / 2) * 2 + 1;

        CarveMaze(sx, sy); // 開始挖洞

        // 設定起點與終點
        startX = 1;
        startY = 1;
        endX = width - 2;
        endY = height - 2;

        maze[startY, startX] = 'S'; // 起點
        maze[endY, endX] = 'E';     // 終點

        playerX = startX;           // 玩家初始位置
        playerY = startY;
        maze[playerY, playerX] = 'P'; // 標記玩家
    }

    // 使用遞迴挖通迷宮
    private void CarveMaze(int x, int y)
    {
        maze[y, x] = ' '; // 當前格子設為通道

        // 定義四個方向（上下左右），以二格為單位移動
        int[][] directions = new int[][]
        {
            new int[]{ 0, -2 }, // 上
            new int[]{ 2, 0 },  // 右
            new int[]{ 0, 2 },  // 下
            new int[]{ -2, 0 }  // 左
        };

        // 隨機打亂方向順序
        Random rand = new Random();
        directions = directions.OrderBy(d => rand.Next()).ToArray();

        // 遍歷每個方向，若鄰格為牆壁則挖通並繼續挖
        foreach (var dir in directions)
        {
            int nx = x + dir[0];
            int ny = y + dir[1];

            if (nx > 0 && nx < maze.GetLength(1) - 1 &&
                ny > 0 && ny < maze.GetLength(0) - 1 &&
                maze[ny, nx] == '#')
            {
                maze[y + dir[1] / 2, x + dir[0] / 2] = ' '; // 挖通中間的牆
                CarveMaze(nx, ny); // 遞迴挖下一格
            }
        }
    }

    // 遊戲開始入口
    public void StartGame()
    {
        Console.WriteLine("歡迎來到隨機迷宮遊戲！");
        Console.WriteLine("使用 W/A/S/D 或方向鍵移動，P 是你的位置");
        Console.WriteLine("# 是牆壁，E 是終點");
        Console.WriteLine($"最多只能撞牆 {maxWallHits} 次，限時 {timeLimit.TotalMinutes} 分鐘");
        Console.WriteLine("按任意鍵開始遊戲...");
        Console.ReadKey(true);

        gameTimer.Start(); // 開始計時
        PlayGame();        // 進入遊戲迴圈
    }

    // 遊戲主迴圈
    private void PlayGame()
    {
        bool gameOver = false;
        bool win = false;

        while (!gameOver)
        {
            Console.Clear();
            PrintMaze();    // 顯示迷宮
            PrintStatus();  // 顯示狀態

            if (CheckWinCondition()) // 勝利條件：到達終點
            {
                gameOver = true;
                win = true;
                break;
            }

            if (CheckLoseConditions()) // 失敗條件：撞牆超過次數或時間到
            {
                gameOver = true;
                win = false;
                break;
            }

            var input = GetInput();     // 取得玩家輸入
            ProcessInput(input);        // 處理輸入移動
        }

        gameTimer.Stop();
        Console.Clear();

        // 顯示結果
        if (win)
        {
            Console.WriteLine("恭喜你贏了！");
            Console.WriteLine($"用時: {gameTimer.Elapsed:mm\\:ss}");
            Console.WriteLine($"撞牆次數: {wallHits}/{maxWallHits}");
        }
        else
        {
            Console.WriteLine("遊戲結束，你輸了！");
            if (wallHits >= maxWallHits)
                Console.WriteLine($"撞牆次數超過限制: {wallHits}/{maxWallHits}");
            else
                Console.WriteLine($"時間用完: {gameTimer.Elapsed:mm\\:ss}/{timeLimit:mm\\:ss}");
        }

        Console.WriteLine("按任意鍵退出...");
        Console.ReadKey(true);
    }

    // 輸出迷宮畫面
    private void PrintMaze()
    {
        for (int y = 0; y < maze.GetLength(0); y++)
        {
            for (int x = 0; x < maze.GetLength(1); x++)
            {
                char c = maze[y, x];

                // 根據字元設顏色
                if (c == 'P')
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write('P');
                }
                else if (c == 'E')
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write('E');
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(c);
                }

                Console.Write(' ');
                Console.ResetColor(); // 重設顏色
            }
            Console.WriteLine();
        }
    }

    // 顯示目前狀態
    private void PrintStatus()
    {
        Console.WriteLine();
        Console.WriteLine($"目前位置: ({playerX}, {playerY})");
        Console.WriteLine($"撞牆次數: {wallHits}/{maxWallHits}");
        Console.WriteLine($"剩餘時間: {(timeLimit - gameTimer.Elapsed).ToString("mm\\:ss")}");
    }

    // 取得玩家輸入（WASD 或 方向鍵）
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

    // 處理移動邏輯與撞牆
    private void ProcessInput(ConsoleKey input)
    {
        int newX = playerX;
        int newY = playerY;

        // 根據輸入計算新位置
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

        // 判斷是否撞牆或走可行路
        if (newX >= 0 && newX < maze.GetLength(1) &&
            newY >= 0 && newY < maze.GetLength(0))
        {
            if (maze[newY, newX] == '#' || maze[newY, newX] == 'S') // 牆或起點不能走
            {
                wallHits++;
                Console.Beep(300, 200);
                Thread.Sleep(200);
            }
            else
            {
                maze[playerY, playerX] = ' '; // 清除原位置
                playerX = newX;
                playerY = newY;
                maze[playerY, playerX] = 'P'; // 更新新位置
            }
        }
        else
        {
            wallHits++;
            Console.Beep(300, 200);
            Thread.Sleep(200);
        }
    }

    // 判斷是否成功到達終點
    private bool CheckWinCondition()
    {
        return playerX == endX && playerY == endY;
    }

    // 判斷是否失敗
    private bool CheckLoseConditions()
    {
        return wallHits >= maxWallHits || gameTimer.Elapsed >= timeLimit;
    }

    // 程式入口點
    static void Main(string[] args)
    {
        TextMazeGame game = new TextMazeGame();
        game.StartGame();
    }
}