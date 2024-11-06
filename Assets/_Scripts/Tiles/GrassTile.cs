using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassTile : Tile
{
    [SerializeField] private Color _baseColor, _offsetColor, _tertiaryColor;

    public override void Init(Vector3Int coordinate)
    {
        //var isOffset = (x + y) % 2 == 1;
        //_renderer.color = isOffset ? _offsetColor : _baseColor;

        var isOffsetCol = coordinate.y % 2 == 1;
        if (isOffsetCol)
        {
            if (coordinate.x % 3 == 0)
            {
                _renderer.color = _baseColor;
            }
            else if (coordinate.x % 3 == 1)
            {
                _renderer.color = _offsetColor;
            }
            else
            {
                _renderer.color = _tertiaryColor;
            }
        }
        else
        {
            if (coordinate.x % 3 == 0)
            {
                _renderer.color = _offsetColor;
            }
            else if (coordinate.x % 3 == 1)
            {
                _renderer.color = _tertiaryColor;
            }
            else
            {
                _renderer.color = _baseColor;
            }
        }
    }
}