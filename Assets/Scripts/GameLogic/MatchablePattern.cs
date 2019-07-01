using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Match3
{


public class MatchablePattern
{
    private static MatchablePattern _intance;
    public static MatchablePattern Instance
    {
        get
        {
            if(null == _intance)
            {
                _intance = new MatchablePattern();
                _intance.Init();
            }

            return _intance;
        }
    }

    private List<List<Point2D>> matchlist = new List<List<Point2D>>();

    private void Init()
    {
        // 오른쪽 이동 시 만들 수 있는 패턴        
        var pattern1 = new List<Point2D>();
        pattern1.Add(new Point2D(0,2)); // x,y 좌표가 아니고 row, col임을 주의
        pattern1.Add(new Point2D(0,3));
        matchlist.Add(pattern1);

        var pattern2 = new List<Point2D>();
        pattern2.Add(new Point2D(1,1));
        pattern2.Add(new Point2D(2,1));
        matchlist.Add(pattern2);

        var pattern3 = new List<Point2D>();
        pattern3.Add(new Point2D(-1,1));
        pattern3.Add(new Point2D(1,1));
        matchlist.Add(pattern3);

        var pattern4 = new List<Point2D>();
        pattern4.Add(new Point2D(-1,1));
        pattern4.Add(new Point2D(-2,1));
        matchlist.Add(pattern4);

        // 아래로 이동 시 만들 수 있는 패턴
        var pattern5 = new List<Point2D>();
        pattern5.Add(new Point2D(2,0)); // new Point2D {row = 2, col =0 }
        pattern5.Add(new Point2D(3,0));
        matchlist.Add(pattern5);
        var pattern6 = new List<Point2D>();
        pattern6.Add(new Point2D(1,1));
        pattern6.Add(new Point2D(1,2));
        matchlist.Add(pattern6);
        var pattern7 = new List<Point2D>();
        pattern7.Add(new Point2D(1,-1));
        pattern7.Add(new Point2D(1,1));
        matchlist.Add(pattern7);
        var pattern8 = new List<Point2D>();
        pattern8.Add(new Point2D(1,-1));
        pattern8.Add(new Point2D(1,-2));
        matchlist.Add(pattern8);

        // 왼쪽 이동 시 만들 수 있는 패턴
        var pattern9 = new List<Point2D>();
        pattern9.Add(new Point2D(0,-2));
        pattern9.Add(new Point2D(0,-3));
        matchlist.Add(pattern9);

        var pattern10 = new List<Point2D> { new Point2D{row=1,col=-1}, new Point2D{row=2,col=-1} };
        var pattern11 = new List<Point2D> { new Point2D{row=-1,col=-1}, new Point2D{row=1,col=-1} };
        var pattern12 = new List<Point2D> { new Point2D{row=-1,col=-1}, new Point2D{row=-2, col=-1} };
        matchlist.Add(pattern10);
        matchlist.Add(pattern11);
        matchlist.Add(pattern12);

        // 위로 이동 시 만들 수 있는 패턴
        var pattern13 = new List<Point2D> { new Point2D{row=-2,col=0}, new Point2D{row=-3,col=0} };
        var pattern14 = new List<Point2D> { new Point2D{row=-1,col=1}, new Point2D{row=-1,col=2} };
        var pattern15 = new List<Point2D> { new Point2D{row=-1,col=-1}, new Point2D{row=-1,col=1} };
        var pattern16 = new List<Point2D> { new Point2D{row=-1,col=-1}, new Point2D{row=-1,col=-2} };
        matchlist.Add(pattern13);
        matchlist.Add(pattern14);
        matchlist.Add(pattern15);
        matchlist.Add(pattern16);

        // 16번 액세스
        //var pos = matchlist[15][0];
        //Debug.LogFormat($"{pos}");
    }

    public Point2D GetPattern(int pattern, int i)
    {
        return matchlist[pattern][i]; 
    }

    public Point2D GetPatternPos(int pattern, int i, int row, int col)
    {
        var calculatedPos = new Point2D(matchlist[pattern][i].row + row, matchlist[pattern][i].col + col);
        return calculatedPos;
    }  

    public int Length
    {
        get { return matchlist.Count; }
    }

}


}
