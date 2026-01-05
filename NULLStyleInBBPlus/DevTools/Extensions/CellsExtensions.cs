using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DevTools.ExtraVariables;

namespace DevTools.Extensions
{
    public static class CellsExtensions
    {
        public static Dictionary<Direction, List<Cell>> DirectionWithMaxFreeCells(this Cell fromCell) {
            var targetPair = new KeyValuePair<Direction, List<Cell>>();
            int maxCount = int.MinValue;

            foreach (var pair in fromCell.GetCellsInAllDirections())
            {
                if (pair.Value.Count > maxCount)
                {
                    maxCount = pair.Value.Count;
                    targetPair = pair;
                }
            }
            return new Dictionary<Direction, List<Cell>>() { { targetPair.Key, targetPair.Value } };
        }

        public static Dictionary<Direction, List<Cell>> DirectionWithMaxFreeCells(this Vector3 fromPos) => ec.CellFromPosition(fromPos).DirectionWithMaxFreeCells();

        public static Cell GetCellOfShape_WithMaxFreeCells(TileShapeMask shape) {
            var cornersInHallway = (from x in ec.mainHall.GetTilesOfShape(shape, true) select x).ToList();

            int max = int.MinValue;
            foreach (var a in cornersInHallway)
            {
                int count = a.DirectionWithMaxFreeCells().ElementAt(0).Value.Count;
                if (count > max) max = count;
            }

            return (from x in cornersInHallway where x.DirectionWithMaxFreeCells().ElementAt(0).Value.Count == max select x).ElementAt(0);
        }

        public static List<Cell> GetCellsInDirection(this Cell startCell, Direction dir) {
            var nextCell = ec.CellFromPosition(startCell.position + dir.ToIntVector2());
            var cellsInDir = new List<Cell>();

            while (nextCell != null && !nextCell.HasWallInDirection(dir))
            {
                cellsInDir.Add(nextCell);
                nextCell = ec.CellFromPosition(nextCell.position + dir.ToIntVector2());
            }
            cellsInDir.Add(nextCell);

            return cellsInDir;
        }

        public static Dictionary<Direction, List<Cell>> GetCellsInAllDirections(this Cell startCell) {
            Dictionary<Direction, List<Cell>> result = new Dictionary<Direction, List<Cell>>();

            foreach (var direction in startCell.AllOpenNavDirections)
            {
                result.Add(direction, startCell.GetCellsInDirection(direction));
            }

            return result;
        }

        public static Cell ToCell(this Vector3 vector) => Singleton<BaseGameManager>.Instance.Ec.CellFromPosition(vector);

        public static Cell ToCell(this Tile tile) {
            var v = IntVector2.GetGridPosition(tile.transform.position);
            return ec.cells[v.x, v.z];
        }

        public static int NavDistanceTo(this Vector3 startPos, Vector3 endPos) => ec.CellFromPosition(startPos).NavDistanceTo(endPos);

        public static int NavDistanceTo(this Cell startCell, Vector3 endPos) => ec.NavigableDistance(startCell, ec.CellFromPosition(endPos), PathType.Nav);
    }
}