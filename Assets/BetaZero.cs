using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine.UIElements;
using UnityEngine.Assertions.Must;
using System;
using static UnityEditor.Experimental.GraphView.GraphView;


public class BetaZero : MonoBehaviour
{

    public int[,] Game;
    public int boardSize = 8;


    public int MiniBatchSize;
    public float LearningCoefficient;
    public float WeigthCoefficiant;
    public float BiasCoefficiant;
    public int SquishMeth;
    public int UpdMeth; // 1 = Normal, 2 = Newton, 3 = InverseNewton

    public static int MaxGameLength = 10;

    AIPlayer aiPlayer1;
    AIPlayer aiPlayer2;
    VisualizeBoard board;


    #region Visual Variables
    public GameObject TilePrefab;
    public GameObject PiecePrefab;
    public int tileSize = 1;

    public Color whiteColor = Color.white;
    public Color blackColor = Color.black;

    public Color BoardWhiteColor;
    public Color BoardBlackColor;

    public GameObject TileParent;
    public GameObject PieceParent;
    #endregion


    void Start()
    { 
        aiPlayer1 = new AIPlayer(boardSize, new int[] { 64, 16, 16, 16, 1 }, 100, 1, 1, 1, 1, 1);
        aiPlayer2 = new AIPlayer(boardSize, new int[] { 64, 16, 16, 16, 1 }, 100, 1, 1, 1, 1, 1);
        aiPlayer1.InitializeNetwork();
        aiPlayer2.InitializeNetwork();
        aiPlayer1.SetupNN();
        aiPlayer2.SetupNN();
        board = new VisualizeBoard(TilePrefab, PiecePrefab, boardSize, tileSize, BoardWhiteColor, BoardBlackColor, whiteColor, blackColor, TileParent, PieceParent);
        board.SetupBoard();
        //Test(board, aiPlayer1, aiPlayer2); ~
        playGames(board, aiPlayer1, aiPlayer2);
    }

    void InitializeVariables()
    {

    }

    void Test(VisualizeBoard board, AIPlayer aiPlayer1, AIPlayer aiPlayer2)
    {
        board.ResetBoard();
        Game = Helpers.ResetBoard(boardSize);
        board.RenderPieces(Game);
        Vector2[] PossibleMoves = Helpers.PossibleMoves(Game, boardSize);
        float[] scores = new float[(int)PossibleMoves[0].x];
        for (int i = 1; i <= PossibleMoves[0].x; i++) //loop threw moves
        {
            Debug.Log(i);
            Debug.Log(PossibleMoves[i]);
            int[,] newGamestate = Helpers.MakeMove(Game, PossibleMoves[i], boardSize);
            float[] Input = aiPlayer1.SetupNetworkInput(newGamestate);
            aiPlayer1.NN(Input); 
            scores[i - 1] = aiPlayer1.GetOutput();
            Debug.Log(scores[i - 1]);
            board.RenderPieces(newGamestate);
        }
        Game = Helpers.MakeMove(Game, PossibleMoves[Array.IndexOf(scores, scores.Max())], boardSize);
    }

    void playGames(VisualizeBoard board, AIPlayer aiPlayer1, AIPlayer aiPlayer2)
    {
        for (int g = 0; g < MiniBatchSize; g++)
        {
            board.ResetBoard();
            Game = Helpers.ResetBoard(boardSize);
            board.RenderPieces(Game);

            int p = 1; // UnityEngine.Random.Range(0, 2); //for debugging always star with p1 normally random ~
            if (p == 0)
            {
                p = -1;
            }
            for (int r = 0; r < MaxGameLength; r++)
            {
                board.ResetBoard();
                board.RenderPieces(Game); //nur rendering ~
                Vector2[] PossibleMoves = Helpers.PossibleMoves(Game, boardSize);
                float[] scores = new float[(int)PossibleMoves[0].x];
                //for (int i = 0; i < PossibleMoves.Length; i++)
                //{
                //    print(PossibleMoves[i]);
                //}
                if (p == 1)
                {
                    for (int i = 1; i <= PossibleMoves[0].x; i++) //loop threw moves
                    {
                        int[,] newGamestate = Helpers.MakeMove(Game, PossibleMoves[i], boardSize);
                        float[] Input = aiPlayer1.SetupNetworkInput(newGamestate);
                        aiPlayer1.NN(Input);
                        scores[i - 1] = aiPlayer1.GetOutput();
                        board.RenderPieces(newGamestate);
                    }
                    print("Scores:");
                    for (int i = 0; i < scores.Length; i++)
                    {
                        print(scores[i]);
                    }
                    print("Max score:");
                    print(scores.Max());
                    print("index:");
                    print(Array.IndexOf(scores, scores.Max()));
                    print("Move:");
                    print(PossibleMoves[Array.IndexOf(scores, scores.Max())]);
                    Game = Helpers.MakeMove(Game, PossibleMoves[Array.IndexOf(scores, scores.Max())], boardSize);
                }
                //else
                //{
                //    for (int i = 1; i <= PossibleMoves[0].x; i++) //loop threw moves
                //    {
                //        int[,] newGamestate = Helpers.MakeMove(Game, PossibleMoves[i], boardSize);
                //        float[] Input = aiPlayer2.SetupNetworkInput(newGamestate);
                //        aiPlayer2.NN(Input);
                //        scores[i - 1] = aiPlayer2.GetOutput();
                //    }
                //    print("Scores:");
                //    print(scores);
                //    Game = Helpers.MakeMove(Game, PossibleMoves[Array.IndexOf(scores, scores.Max())], boardSize);
                //}
                //if (Helpers.GameTerminatet(Game, boardSize) == 1)
                //{
                //    if (p == 1)
                //    {
                //
                //    }
                //    break;
                //}
                //p *= -1;
                //Game = Helpers.FlipBoard(Game, boardSize);
                ////Todo: Make Game recording and distribute it
                break;
            }
            break;

        }
        board.ResetBoard();
        board.RenderPieces(Game); //nur rendering ~
    }

    // Update is called once per frame
    void Update()
    {
        //playGames(board, aiPlayer1, aiPlayer2);
    }
}

public static class Helpers
{

    public struct LabeledData
    {
        public float[] Data;
        public float[] Label;
        public LabeledData(float[] Data, float[] Label)
        {
            this.Data = Data;
            this.Label = Label;
        }
    }

    public static int[,] FlipBoard(int[,] Board, int boardSize)
    {
        int[,] newBoard = new int[boardSize, boardSize];
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                newBoard[i, j] = -Board[i, j];
            }
        }
        return newBoard;
    }


    public static readonly Vector2[] Directions =
    {
       new Vector2(-1, -1), new Vector2(0, -1), new Vector2(1, -1), // Up-left, Up, Up-right
       new Vector2(-1, 0),  new Vector2(1, 0),                      // Left, Right
       new Vector2(-1, 1),  new Vector2(0, 1),  new Vector2(1, 1)   // Down-left, Down, Down-right
    };



    public static int[,] ResetBoard(int boardSize)
    {
        int[,] Output = new int[boardSize, boardSize];
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                Output[i, j] = 0;
            }
        }
        int x = (int)boardSize / 2;
        Output[x, x] = -1;
        Output[x + 1, x] = 1;
        Output[x, x + 1] = 1;
        Output[x + 1, x + 1] = -1;
        return Output;
    }

    public static bool IsValidMove(int[,] gameState, Vector2 move, int boardSize)
    {
        int opponent = -1;
        int myPiece = 1;

        int x = (int)move.x;
        int y = (int)move.y;


        // Move must be inside bounds and on an empty space
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize || gameState[x, y] != 0)
        {
            return false;
        }


        // Check all 8 directions
        foreach (var dir in Directions)
        {
            int dx = (int)dir.x, dy = (int)dir.y;
            int stepX = x + dx, stepY = y + dy;
            bool foundOpponent = false;


            // Move in this direction while encountering opponent pieces
            while (stepX >= 0 && stepX < boardSize && stepY >= 0 && stepY < boardSize)
            {
                if (gameState[stepX, stepY] == opponent)
                {
                    foundOpponent = true; // We must flip at least one opponent piece
                }
                else if (gameState[stepX, stepY] == myPiece)
                {
                    if (foundOpponent) return true; // If found opponent pieces before my piece, it's valid
                    break; // Otherwise, it's an invalid direction
                }
                else break; // Encountering empty space stops this direction


                stepX += dx;
                stepY += dy;
            }
        }
        return false; // No valid flipping found
    }

    public static float Sigmoid(float x)
    {
        return x;
        //return 2f *(1f / (1f + Mathf.Exp(-x))) - 1;
    }

    public static float D_Sigmoid(float x)
    {
        float y = Sigmoid(x);
        return 2*(y * (1f -y));
    }

    public static float CostFunction(float OUTPUT, float y)
    {
        return (y - OUTPUT) * (y - OUTPUT);
    }

    public static float OverallCost(float[] OUTPUT, float[] y)
    {
        float Cost = 0;
        for (int i = 0; i < OUTPUT.Length; i++)
        {
            Cost += CostFunction(OUTPUT[i], y[i]);
        }
        return Cost;
    }

    public static void MakeMiniBatch(int Size)
    {

    }

    public static int Round(float x)
    {
        if (x - (int)x < 0.5f)
        {
            return (int)x;
        }
        return ((int)x + 1);
    }

    public static int[,] MakeMove(int[,] gameState, Vector2 move, int boardSize)
    {
        int[,] Output = gameState;
        int x = (int)move.x;
        int y = (int)move.y;


        Output[x, y] = 1; // Place the piece


        // Flip opponent pieces in valid directions
        foreach (var dir in Directions)
        {
            int dx = (int)dir.x, dy = (int)dir.y;
            int stepX = x + dx, stepY = y + dy;
            bool foundOpponent = false;
            var toFlip = new System.Collections.Generic.List<Vector2>();


            // Move in this direction while encountering opponent pieces
            while (stepX >= 0 && stepX < boardSize && stepY >= 0 && stepY < boardSize)
            {
                if (gameState[stepX, stepY] == -1)
                {
                    foundOpponent = true;
                    toFlip.Add(new Vector2(stepX, stepY)); // Store pieces to flip
                }
                else if (gameState[stepX, stepY] == 1)
                {
                    if (foundOpponent)
                    {
                        // Flip all opponent pieces in this direction
                        foreach (var pos in toFlip)
                            gameState[(int)pos.x, (int)pos.y] = 1;
                    }
                    break; // Stop searching in this direction
                }
                else break; // Empty space stops flipping


                stepX += dx;
                stepY += dy;
            }
        }
        return Output;
    }

    public static int GameTerminatet(int[,] gameState, int boardSize)
    {
        int x = 0;
        int y = 0;
        int z = 0;
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (gameState[i, j] == 1)
                {
                    x++;
                    z++;
                }
                else if (gameState[i, j] == -1)
                {
                    y++;
                    z++;
                }
            }
        }
        if (x == 0)
        {
            return -1;
        }
        if (y == 0)
        {
            return 1;
        }
        if (z == boardSize * boardSize)
        {
            if (x < y)
            {
                return -1;
            }
            if (y < x)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
        int w = 0;
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (IsValidMove(gameState, new Vector2(i, j), boardSize)) 
                {
                    w++;
                }
            }
        }
        if (w == 0)
        {
            return -1;
        }
        return 0;
        //1 = I won
        //-1 = opponent won
        //0 = the Game is still running
        //2 = draw
    }

    public static int[,] ExtractSlice(int[,][] Array, int n, int Size)
    {
        int[,] newArray = null;
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                newArray[i, j] = Array[i, j][n];
            }
        }
        return newArray;
    }

    public static int[,][] PrintSlice(int[,][] Array, int[,] Slice, int n, int Size)
    {
        int[,][] newArray = Array;
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                newArray[i, j][n] = Slice[i, j];
            }
        }
        return newArray;
    }

    public static float[] ExtractSlice1D(float[,] Array, int Layer, int LayerSize)
    {
        float[] newArray = new float[LayerSize];
        for (int i = 0; i < LayerSize; i++)
        {
            newArray[i] = Array[Layer, i];
        }
        return newArray;
    }

    public static Vector2[] PossibleMoves(int[,] gameState, int boardSize)
    {
        Vector2[] possMoves = new Vector2[boardSize * boardSize];
        int x = 0;
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (IsValidMove(gameState, new Vector2(i, j), boardSize))
                {
                    possMoves[x + 1] = new Vector2(i, j);
                    x++;
                }
            }
        }
        possMoves[0] = new Vector2(x, 0); //indicates how many possible moves there are
        return possMoves;
    }

    public static float UpdatingMethode(int UpdMeth, float x, float D_x, float cost, float SpecificCoefficient, float LearningCoefficient)
    {
        if (UpdMeth == 1)
        {
            return StepwiseUpdating(x, D_x, cost, SpecificCoefficient, LearningCoefficient);
        }
        else if (UpdMeth == 2)
        {
            return NewtonUpdating(x, D_x, cost, SpecificCoefficient, LearningCoefficient);
        }
        else
        {
            return InverseNewtonUpdating(x, D_x, cost, SpecificCoefficient, LearningCoefficient);
        }
    }

    public static float StepwiseUpdating(float x, float D_x, float cost, float SpecificCoefficient, float LearningCoefficient)
    {
        float y = x - (LearningCoefficient * SpecificCoefficient * D_x);
        return y;
    }

    public static float NewtonUpdating(float x, float D_x, float cost, float SpecificCoefficient, float LearningCoefficient)
    {
        float y = x - (LearningCoefficient * SpecificCoefficient * cost / D_x);
        return y;
    }

    public static float InverseNewtonUpdating(float x, float D_x, float cost, float SpecificCoefficient, float LearningCoefficient)
    {
        float y = x - (LearningCoefficient * SpecificCoefficient * cost * D_x);
        return y;
    }
    public static void DeleteChildren(GameObject Parent)
    {
        var children = new List<GameObject>();

        foreach (Transform child in Parent.transform) //geht irgendwie nicht leichter
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            UnityEngine.Object.Destroy(child);
        }
    }
}

public class VisualizeBoard
{
    public GameObject TilePrefab;
    public GameObject PiecePrefab;
    public int boardSize;
    public Vector2 startPoint = new Vector2(0, 0);
    private GameObject[,] Tiles;
    private GameObject[,] Pieces;
    public int tileSize = 1;

    public Color BoardWhiteColor;
    public Color BoardBlackColor;

    public Color whiteColor;
    public Color blackColor;

    private GameObject TileParent;
    private GameObject PieceParent;

    public VisualizeBoard(GameObject tilePrefab, GameObject piecePrefab,  int boardSize, int tileSize, Color BoardWhite, Color BoardBlack, Color whiteColor, Color blackColor, GameObject TileParent, GameObject PieceParent)
    {
        TilePrefab = tilePrefab;  //ahhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh
        PiecePrefab = piecePrefab;
        this.boardSize = boardSize;
        this.startPoint = new Vector2(-(boardSize / 2 * tileSize) + 0.5f, -(boardSize / 2 * tileSize) + 0.5f);
        this.tileSize = tileSize;
        this.BoardWhiteColor = BoardWhite;
        this.BoardBlackColor = BoardBlack;
        this.whiteColor = whiteColor;
        this.blackColor = blackColor;
        this.PieceParent = PieceParent;
        this.TileParent = TileParent;
    }

    public void SetupBoard()
    {
        Tiles = new GameObject[boardSize, boardSize];
        Vector2 position = startPoint;
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                position = startPoint + new Vector2(i * tileSize, j * tileSize);
                if (i % 2 ==j % 2)
                {
                    TilePrefab.GetComponent<SpriteRenderer>().color = BoardWhiteColor;
                }
                else
                {
                    TilePrefab.GetComponent<SpriteRenderer>().color = BoardBlackColor;
                }
                Tiles[i, j] = GameObject.Instantiate(TilePrefab, position, Quaternion.identity);
                Tiles[i, j].transform.SetParent(TileParent.transform);
                Tiles[i, j].transform.name = "Tile" + (i * 8 + j + 1);
                
            }
        }
    }

    public void RenderPieces(int[,] gameState)
    {
        Pieces = new GameObject[boardSize, boardSize];
        Vector2 position = startPoint;
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (gameState[i, j] != 0)
                {
                    position = startPoint + new Vector2(i * tileSize, j * tileSize);

                    if (gameState[i, j] == 1)
                    {
                        PiecePrefab.GetComponent<SpriteRenderer>().color = whiteColor;
                    }
                    else
                    {
                        PiecePrefab.GetComponent<SpriteRenderer>().color = blackColor;
                    }
                    Pieces[i, j] = GameObject.Instantiate(PiecePrefab, position, Quaternion.identity);
                    Pieces[i, j].transform.SetParent(PieceParent.transform);
                    Pieces[i, j].transform.name = "Piece" + (i * 8 + j + 1);
                }
            }
        }
    }
 
    public void DeleteBoard()
    {
        Helpers.DeleteChildren(TileParent);
    }

    public void ResetBoard()
    {
        
        Helpers.DeleteChildren(PieceParent);
    }
}

public class AIPlayer
{
    #region Variable Declarations and setup
    public struct LabeledData
    {
        public float[] Data;
        public float[] Label;
        public LabeledData(float[] Data, float[] Label)
        {
            this.Data = Data;
            this.Label = Label;
        }
    }

    //Reversi Bot
    public int[] Layers;
    public float[,] Neuron; // Layer, Neuron
    public float[,] Bias;
    public float[,,] Weight; // Layer, Neuron, prev Neuron
    public float[,] z;


    public int[,] Game;
    public int boardSize;


    public int MiniBatchSize;
    public float LearningCoefficient;
    public float WeigthCoefficiant;
    public float BiasCoefficiant;
    public int SquishMeth;
    public int UpdMeth; // 1 = Normal, 2 = Newton, 3 = InverseNewton


    private float[,] D_A;
    private float[,] D_Z;
    private float[,] D_B;
    private float[,,] D_W;
    private float Cost;


    private float[,][] L_D_A;  // List of differencials for different training examples
    private float[,][] L_D_Z;
    private float[,][] L_D_B;
    private float[,,][] L_D_W;
    private float[] L_Cost;


    private float[,] A_D_A;  // Average of differencials over different training examples
    private float[,] A_D_Z;
    private float[,] A_D_B;
    private float[,,] A_D_W;
    private float A_Cost;


    private LabeledData[] TrainingData;
    private LabeledData[] MiniBatch;
    private LabeledData CurrentData;




    public int[,][] CurrentRecordedGame;
    public int[,][][] CurrentMiniBatchGameRecording;
    #endregion

    public AIPlayer(int boardSize, int[] Layers, int MiniBatchSize, float LearningCoefficient, float WeigthCoefficiant, float BiasCoefficiant, int SquishMeth, int UpdMeth)
    {
        this.boardSize = boardSize;
        this.Layers = Layers;
        this.MiniBatchSize = MiniBatchSize;
        this.LearningCoefficient = LearningCoefficient;
        this.WeigthCoefficiant = WeigthCoefficiant;
        this.BiasCoefficiant = BiasCoefficiant;
        this.SquishMeth = SquishMeth;  //not implemented yet ~
        this.UpdMeth = UpdMeth;
        this.Layers[Layers.Length - 1] = 1;
    }


    public void InitializeNetwork()
    {
        Neuron = new float[Layers.Length, Layers.Max()]; // Layer, Neuron
        Bias = new float[Layers.Length, Layers.Max()];
        Weight = new float[Layers.Length, Layers.Max(), Layers.Max()]; // Layer, Neuron, prev Neuron
        z = new float[Layers.Length, Layers.Max()]; //could be one Layer less romve later ~


        D_A = new float[Layers.Length, Layers.Max()];
        D_Z = new float[Layers.Length, Layers.Max()];
        D_B = new float[Layers.Length, Layers.Max()];
        D_W = new float[Layers.Length, Layers.Max(), Layers.Max()];


        L_D_A = new float[Layers.Length, Layers.Max()][];  // List of differencials for different training examples
        L_D_Z = new float[Layers.Length, Layers.Max()][];
        L_D_B = new float[Layers.Length, Layers.Max()][];
        L_D_W = new float[Layers.Length, Layers.Max(), Layers.Max()][];
        L_Cost = new float[MiniBatchSize];


        A_D_A = new float[Layers.Length, Layers.Max()];
        A_D_Z = new float[Layers.Length, Layers.Max()];
        A_D_B = new float[Layers.Length, Layers.Max()];
        A_D_W = new float[Layers.Length, Layers.Max(), Layers.Max()];


        LabeledData[] TrainingData = new LabeledData[1]; // ~
        LabeledData[] MiniBatch = new LabeledData[MiniBatchSize];


        CurrentRecordedGame = new int[boardSize, boardSize][]; //Maximum duration of game is
        CurrentMiniBatchGameRecording = new int[boardSize, boardSize][][];
    }

    public float[] SetupNetworkInput(int[,] GameState) //prob should go into Helpers but idc ~
    {
        return GameState.Cast<int>()
                .Select(i => (float)i)
                .ToArray();
    }

    public void NN(float[] Input)
    {
        Neuron = new float[Layers.Length, Layers.Max()]; //remove after debugging ~
        for (int i = 0; i < Layers[0]; i++)
        {
            Neuron[0, i] = Input[i];
        }
        for (int i = 1; i < Layers.Length; i++)
        {
            for (int j = 0; j < Layers[i]; j++)
            {
                z[i, j] = 0;
                float result = 0;
                for (int k = 0; k < Layers[i - 1]; k++)
                {
                    //Debug.Log("i=" + i + ",j=" + j + ",k=" + k);
                    float w = Weight[i, j, k];
                    float n = Neuron[i - 1, k];
                    //Debug.Log(result + " + " + w + " * " + n + " = ");
                    result += w * n;
                    //z[i, j] += Weight[i, j, k] * Neuron[i - 1, k];
                    //Debug.Log(result);
                }
                z[i, j] = result;
                //Debug.Log(" +=  " + Bias[i, j] + " = ");
                z[i, j] += Bias[i, j];
                //Debug.Log(z[i, j]);
                Neuron[i, j] = Helpers.Sigmoid(z[i, j]);
            }
        }
        //Debug.Log("NN Output");
        //Debug.Log(Neuron[Layers.Length - 1, 0]);
    }

    public float GetOutput()
    {
        float Output;
        Output = Neuron[Layers.Length - 1, 0];
        return Output;   //only for debugging can be condensed later
    }
    public void SetupNN()
    {
        Bias = new float[Layers.Length, Layers.Max()];
        Weight = new float[Layers.Length, Layers.Max(), Layers.Max()];
        float rand = 0; //for debuggging, remove later... ill prob forgor
        int num = 0;
        for (int i = 1; i < Layers.Length; i++)
        {
            for (int j = 0; j < Layers[i]; j++)
            {
                rand = UnityEngine.Random.value * 2 - 1;
                Bias[i, j] = rand;
                for (int k = 0; k < Layers[i - 1]; k++)
                {
                    rand = UnityEngine.Random.value * 2 - 1;
                    Weight[i, j, k] =  rand;
                    num++;
                }
            }
        }
    }

    public void Backpropagation(LabeledData CurrentExample)
    {
        float[] Output = new float[Layers[Layers.Length - 1]];
        for (int i = 0; i < Layers[Layers.Length - 1]; i++)
        {
            Output[i] = Neuron[Layers.Length - 1, i];
        }
        Cost = Helpers.OverallCost(Output, CurrentExample.Label);
        for (int i = 0; i < Layers[Layers.Length - 1]; i++) // Last Row D_Activation
        {
            D_A[Layers.Length - 1, i] = 2 * (Output[i] - CurrentExample.Label[i]);
        }
        for (int i = Layers.Length - 1; i > -1; i--)  // Step back threw Layers
        {
            for (int j = 0; j < Layers[i]; j++)
            {
                D_Z[i, j] = Helpers.D_Sigmoid(z[i, j]) * D_A[i, j]; // Compute derivative of z in respect to the Cost
                D_B[i, j] = D_Z[i, j];  // Compute derivative of the Bias in respect to the Cost
                for (int k = 0; k < Layers[i - 1]; k++)
                {
                    D_W[i, j, k] = Neuron[i - 1, k] * D_Z[i, j];   // Compute derivative of the Weight in respect to the Cost
                }
            }
            for (int j = 0; j < Layers[i]; j++) // Compute derivative of the Activation of the Neurons in the previous Layer in respect to the Cost
            {
                D_A[i - 1, j] = 0;
                for (int k = 0; k < Layers[i]; k++) // sum up all ways one Neuron influences the next layer
                {
                    D_A[i - 1, j] += Weight[i, k, j] * D_Z[i, k]; // indeces seem to be the wrong way around, because they reference the Neuron of the previous Layer
                }
            }
        }
    }

    public void FeedTrainingExamples()
    {
        Helpers.MakeMiniBatch(MiniBatchSize);
        for (int h = 0; h < MiniBatchSize; h++)
        {
            NN(MiniBatch[h].Data);
            Backpropagation(MiniBatch[h]);
            for (int i = 0; i > Layers.Length - 1; i++)
            {
                for (int j = 0; j < Layers[i]; j++)
                {
                    L_D_A[i, j][h] = D_A[i, j];
                    L_D_Z[i, j][h] = D_Z[i, j];
                    L_D_B[i, j][h] = D_B[i, j];
                    for (int k = 0; k < Layers[i - 1]; k++)
                    {
                        L_D_W[i, j, k][h] = D_W[i, j, k];
                    }
                }
            }
            L_Cost[h] = Cost;
        }
        A_D_A = null;
        A_D_Z = null;
        A_D_B = null;
        A_D_W = null;
        A_Cost = 0;
        for (int h = 0; h < MiniBatchSize; h++)
        {
            for (int i = 0; i > Layers.Length - 1; i++)
            {
                for (int j = 0; j < Layers[i]; j++)
                {
                    A_D_A[i, j] += L_D_A[i, j][h] / MiniBatchSize;
                    A_D_Z[i, j] += L_D_Z[i, j][h] / MiniBatchSize;
                    A_D_B[i, j] += L_D_B[i, j][h] / MiniBatchSize;
                    for (int k = 0; k < Layers[i - 1]; k++)
                    {
                        A_D_W[i, j, k] = L_D_W[i, j, k][h];
                    }
                }
            }
            A_Cost += L_Cost[h] / MiniBatchSize;
        }
    }


    public void AdjustNeuralNetwork()
    {
        for (int i = Layers.Length - 1; i > -1; i--)  // Step back threw Layers
        {
            for (int j = 0; j < Layers[i]; j++)
            {
                Bias[i, j] = Helpers.UpdatingMethode(UpdMeth, Bias[i, j], A_D_B[i, j], A_Cost, BiasCoefficiant, LearningCoefficient);
                for (int k = 0; k < Layers[i - 1]; k++)
                {
                    Weight[i, j, k] = Helpers.UpdatingMethode(UpdMeth, Weight[i, j, k], A_D_W[i, j, k], A_Cost, WeigthCoefficiant, LearningCoefficient);
                }
            }
        }
    }
}


