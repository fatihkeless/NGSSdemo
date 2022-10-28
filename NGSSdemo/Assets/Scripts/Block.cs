using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{
    


    public int value;
    public Node _node;
    public Block mergingBlock;
    public bool Merging;
    public Vector2 Pos => transform.position;
    public SpriteRenderer _renderer;   
    [SerializeField] private TextMeshPro text;
    public void Inýt(BlockType type)
    {
        value = type.Value;
        _renderer.color = type.Color;
        text.text = type.Value.ToString();

    }

    public void SetBlock(Node node)
    {
        if (_node != null) _node.NonFreeBlock = null;
        _node = node;
        _node.NonFreeBlock = this;
    }


    public void MergeBlock(Block blockToMergeWith)
    {
        mergingBlock = blockToMergeWith;



        _node.NonFreeBlock = null;


        blockToMergeWith.Merging = true;

    }


    public bool CanMerge(int _value) => _value == value && !Merging && mergingBlock == null;

}
