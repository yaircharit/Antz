using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BlockType { Grass };
public enum BlockSide { Top, Bottom, North, South, East, West };

public static class BlockData // TODO: Consider making this class to a base for Block class
{
    public static readonly Vector3Int[] vertices = new Vector3Int[8] {
        new Vector3Int(0,0,0),// 0         7-----------3
        new Vector3Int(0,0,1),// 1        /|          /|          U   E
        new Vector3Int(0,1,0),// 2       / |         / |        Y+|  / Z+
        new Vector3Int(0,1,1),// 3      6--+--------2  |          | /
        new Vector3Int(1,0,0),// 4      |  5--------|--1    X+    |/ 
        new Vector3Int(1,0,1),// 5      | /         | /   N <-----+----- S
        new Vector3Int(1,1,0),// 6      |/          |/           /|    X-
        new Vector3Int(1,1,1),// 7      4-----------0         Z-/ |Y-
    };                     //                               W  D


    public static readonly int[,] sideVertices = new int[,] {
        { 2, 3, 6, 7 }, // Top
        { 1, 0, 5, 4 }, // Bottom (Counter CW)
        { 5, 4, 7, 6 }, // North
        { 0, 1, 2, 3 }, // South
        { 1, 5, 3, 7 }, // East
        { 4, 0, 6, 2 }  // West
    };

    public static readonly int[] trianglesTemplate = new int[] {
        2, 1, 3, 2, 0, 1
    };


    public static readonly Vector2[] uvs = new Vector2[] {
        new Vector2(0,1),
        new Vector2(1,1),
        new Vector2(1,0),
        new Vector2(0,0)
    };

    public static readonly Vector3Int[] neighbors = new Vector3Int[] {
        new Vector3Int(0,1,0), // Top
        new Vector3Int(0,-1,0),// Bottom
        new Vector3Int(0,0,1), // North (Z+)
        new Vector3Int(0,0,-1),// South (Z-)
        new Vector3Int(1,0,0), // East (X+)
        new Vector3Int(-1,0,0) // West (X-)
    };

}