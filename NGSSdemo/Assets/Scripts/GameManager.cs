using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Security.Cryptography;
using System;
using DG.Tweening;

public enum Swipe { None, swipeLeft, swipeRight, swipeUp, swipeDown }

public enum GameState
{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}


public class GameManager : MonoBehaviour
{

    Vector2 startPos, endPos;

    private GameState _state;
    public int _round;
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    private int winCond = 32;
     public List<BlockType> _types;

    private List<Node> _nodes;
    private List<Block> _blocks;

    public GameObject winScreen;
    public GameObject loseScreen;


    private BlockType GetBlockTypeByValue(int value) => _types.First(t => t.Value == value);

    // Start is called before the first frame update
    void Start()
    {

        ChangeState(GameState.GenerateLevel);
    }

    private void Update()
    {
       


        if (_state != GameState.WaitingInput) return;

       

        onUpdate();

    }

   



    private void ChangeState(GameState newState)
    {
        _state = newState;

        switch (newState)
        {
            case GameState.GenerateLevel:
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks(_round++ == 0 ? 1 : 1);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                winScreen.SetActive(true);
                break;
            case GameState.Lose:
                loseScreen.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }


    void GenerateGrid()
    {

        _nodes = new List<Node>();
        _blocks = new List<Block>();

         

        for(int x= 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity);
                _nodes.Add(node);
            }



            
        }

        var center = new Vector2((float)width / 2 - 0.5f, (float)height / 2 - 0.5f);

        var board = Instantiate(boardPrefab, Vector3.zero, Quaternion.identity);
        // board.transform.localScale = new Vector2(width,height);
        board.size = new Vector2(width, height);
        board.transform.position = center;
        SpawnBlocks(1);

    }    



    // bloklarý spawn etme spawn edilecek blok yeri kalmadýysada oyunu sonlandýrma ve skoru tutma
    void SpawnBlocks(int amount)
    {

        var freeNodes = _nodes.Where(n => n.NonFreeBlock == null).OrderBy(b => 1).ToList();


        foreach(var node in freeNodes.Take(amount))
        {


            spawnBlock(node, 2);
            PauseScript.score += 2;

        }


    
        if(freeNodes.Count() == 1)
        {
            //lose game
            // player prefs kullanmý olacak
            Debug.Log("Game Over");
            ChangeState(GameState.Lose);
            return;
        }

        ChangeState(_blocks.Any(b => b.value == winCond ) ? GameState.Win : GameState.WaitingInput);

    
    
    }


    void spawnBlock(Node node,int value)
    {
        var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
        block.Inýt(GetBlockTypeByValue(value));
        block.SetBlock(node);
        _blocks.Add(block);
    }


    void Shift(Vector2 dir)
    {
        ChangeState(GameState.Moving);
        var orderedBlocks = _blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();


        
        foreach(var block in orderedBlocks)
        {
            var next = block._node;
            do
            {
                block.SetBlock(next);

                var possibleNode = GetNodeAtPosition(next.Pos + dir);

                if(possibleNode != null)
                {
                    if(possibleNode.NonFreeBlock != null && possibleNode.NonFreeBlock.CanMerge(block.value))
                    {
                        block.mergingBlock = possibleNode.NonFreeBlock;
                    }



                    if (possibleNode.NonFreeBlock == null) next = possibleNode;
                }


            } while (next != block._node);


           // block.transform.DOMove(block._node.Pos, 0.2f);


        }

        var sequence = DOTween.Sequence();

        foreach(var block in orderedBlocks)
        {
            var movePoint = block.Merging ? block.mergingBlock.Pos : block._node.Pos;

            sequence.Insert(0, block.transform.DOMove(block._node.Pos, 0.2f));

        }

        sequence.OnComplete(() =>
        {

            foreach (var block in orderedBlocks.Where(b => b.mergingBlock != null))
            {
                mergeBlock(block, block.mergingBlock);
            }

            ChangeState(GameState.SpawningBlocks);

        });



    }

    void mergeBlock(Block baseBlock,Block mergingBlock)
    {
        spawnBlock(mergingBlock._node, mergingBlock.value * 2);
        PauseScript.score += mergingBlock.value;
        
        RemoveBlock(mergingBlock);
        RemoveBlock(baseBlock);
    }

    void RemoveBlock(Block block)
    {
        _blocks.Remove(block);
        Destroy(block.gameObject);
    }


    Node GetNodeAtPosition(Vector2 pos)
    {

        return _nodes.FirstOrDefault(n => n.Pos == pos);
    }


    #region swipeController


    public void onUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            endPos = startPos;

        }
        else if (Input.GetMouseButtonUp(0))
        {
            endPos = Input.mousePosition;

            if (Vector2.Distance(endPos, startPos) > 100f)
            {
                SwipeDirection();
            }
        }


        
    }


    public Swipe SwipeDirection()
    {
        Swipe direction = Swipe.None;


        Vector2 currentSwipe = endPos - startPos;

        float angle = Mathf.Atan2(currentSwipe.y, currentSwipe.x) * (180 / Mathf.PI);


        if (angle > 67.5f && angle < 112.5f)
        {
            direction = Swipe.swipeUp;
            Shift(Vector2.up);
            Debug.Log("Up");
            //  arrow = Instantiate(_levelManager.directArrow, currentPencil.transform.position, Quaternion.Euler(0, 0, 0));

        }
        else if (angle < -67.5f && angle > -112.5f)
        {
            direction = Swipe.swipeDown;
            Shift(Vector2.down);
            Debug.Log("down");
            //  arrow = Instantiate(_levelManager.directArrow, currentPencil.transform.position, Quaternion.Euler(0, 0, 180));
        }
        else if (angle < -157.5f || angle > 157.5f)
        {
            direction = Swipe.swipeLeft;
            Shift(Vector2.left);
            Debug.Log("left");
            //   arrow = Instantiate(_levelManager.directArrow, currentPencil.transform.position, Quaternion.Euler(0, 0, 90));
        }
        else if (angle > -22.5f && angle < 22.5f)
        {
            direction = Swipe.swipeRight;
            Shift(Vector2.right);
            Debug.Log("Right");
            //  arrow = Instantiate(_levelManager.directArrow, currentPencil.transform.position, Quaternion.Euler(0, 0, 270));
        }
        


        return direction;

    }

    #endregion




}








[Serializable]
public struct BlockType
{
    public int Value;
    public Color Color;
}