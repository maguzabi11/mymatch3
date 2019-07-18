using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Match3
{

// 검사용 패턴 클래스

public class MatchablePattern : PatternBase
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

    protected override void Init()
    {
        // 오른쪽 이동 시 만들 수 있는 패턴        
        var pattern1 = new List<Point2D>();
        pattern1.Add(new Point2D(0,2)); // x,y 좌표가 아니고 row, col임을 주의
        pattern1.Add(new Point2D(0,3));
        matchlist.Add(pattern1);
        matchlist.Add(new List<Point2D> { new Point2D(0,-2), new Point2D(0,1) });
        matchlist.Add(new List<Point2D> { new Point2D(0,-3), new Point2D(0,-1) });

        matchlist.Add(new List<Point2D> { new Point2D(1,1), new Point2D(2,1) });
        matchlist.Add(new List<Point2D> { new Point2D(-1,-1), new Point2D(1,0) });
        matchlist.Add(new List<Point2D> { new Point2D(-2,-1), new Point2D(-1,0) });

        matchlist.Add(new List<Point2D> { new Point2D(-1,1), new Point2D(1,1) });
        matchlist.Add(new List<Point2D> { new Point2D(1,-1), new Point2D(2,0) });
        matchlist.Add(new List<Point2D> { new Point2D(-1,-1), new Point2D(-2,0) });

        /*
         B
         B
        B_
         */
        matchlist.Add(new List<Point2D>{new Point2D(-1,1), new Point2D(-2,1)});
        matchlist.Add(new List<Point2D>{new Point2D(1,0), new Point2D(2,-1)});
        matchlist.Add(new List<Point2D>{new Point2D(-1,0), new Point2D(1,-1)});

        // 아래로 이동 시 만들 수 있는 패턴

        /*
          B
          _
          B
          B
         */
        matchlist.Add(new List<Point2D>{ new Point2D(2,0), new Point2D(3,0)});
        matchlist.Add(new List<Point2D>{ new Point2D(-2,0), new Point2D(1,0)});
        matchlist.Add(new List<Point2D>{ new Point2D(-3,0), new Point2D(-1,0)});

        /*
        B
         _BB
        */
        matchlist.Add(new List<Point2D>{new Point2D(1,1), new Point2D(1,2)});
        matchlist.Add(new List<Point2D>{new Point2D(-1,-2), new Point2D(0,1)});
        matchlist.Add(new List<Point2D>{new Point2D(-1,-3), new Point2D(0,-1)});

        /*
         B
        B_B
         */
        matchlist.Add(new List<Point2D> {new Point2D(1,-1), new Point2D(1,1)} );
        matchlist.Add(new List<Point2D> {new Point2D(-1,1), new Point2D(0,2)} );
        matchlist.Add(new List<Point2D> {new Point2D(0,-2), new Point2D(-1,-1)} );

        /*
          B
        BB_
         */
        matchlist.Add(new List<Point2D> {new Point2D(1,-1), new Point2D(1,-2)});
        matchlist.Add(new List<Point2D> {new Point2D(0,1), new Point2D(-1,2)});
        matchlist.Add(new List<Point2D> {new Point2D(0,-1), new Point2D(-1,1)});

        // 왼쪽 이동 시 만들 수 있는 패턴
        /*
        BB_B
         */
        matchlist.Add(new List<Point2D> {new Point2D(0,-2), new Point2D(0,-3)});
        matchlist.Add(new List<Point2D> {new Point2D(0,1), new Point2D(0,3)});
        matchlist.Add(new List<Point2D> {new Point2D(0,-1), new Point2D(0,2)});

        /*
        _B
        B
        B
         */
        matchlist.Add(new List<Point2D> { new Point2D(1,-1), new Point2D(2,-1)});
        matchlist.Add(new List<Point2D> { new Point2D(-1,1), new Point2D(1,0)});
        matchlist.Add(new List<Point2D> { new Point2D(-1,0), new Point2D(-2,1)});

        /*
        B
        _B
        B
         */
        matchlist.Add(new List<Point2D>{ new Point2D(-1,-1), new Point2D(1,-1) });
        matchlist.Add(new List<Point2D>{ new Point2D(1,1), new Point2D(2,0) });
        matchlist.Add(new List<Point2D>{ new Point2D(-1,1), new Point2D(-2,0) });

        /*
        B
        B
        _B
         */
        matchlist.Add(new List<Point2D> { new Point2D(-1,-1), new Point2D(-2,-1) });
        matchlist.Add(new List<Point2D> { new Point2D(1,0), new Point2D(2,1) });
        matchlist.Add(new List<Point2D> { new Point2D(-1,0), new Point2D(1,1) });

        // 위로 이동 시 만들 수 있는 패턴

        /*
        B
        B
        _
        B
         */
        matchlist.Add(new List<Point2D> { new Point2D(-2,0), new Point2D(-3,0) });
        matchlist.Add(new List<Point2D> { new Point2D(1,0), new Point2D(3,0) });
        matchlist.Add(new List<Point2D> { new Point2D(-1,0), new Point2D(2,0) });

        /*
        _BB
        B
         */
        matchlist.Add(new List<Point2D> { new Point2D(-1,1), new Point2D(-1,2) });
        matchlist.Add(new List<Point2D> { new Point2D(1,-1), new Point2D(0,1) });
        matchlist.Add(new List<Point2D> { new Point2D(1,-2), new Point2D(0,-1) });

        /*
        B_B
         B
         */
        matchlist.Add(new List<Point2D> { new Point2D(-1,-1), new Point2D(-1,1) });
        matchlist.Add(new List<Point2D> { new Point2D(1,1), new Point2D(0,2) });
        matchlist.Add(new List<Point2D> { new Point2D(1,-1), new Point2D(0,-2) });

        /*
        BB_
          B
         */
        matchlist.Add(new List<Point2D>{ new Point2D(-1, -1), new Point2D(-1,-2) });
        matchlist.Add(new List<Point2D>{ new Point2D(0, 1), new Point2D(1,2) });
        matchlist.Add(new List<Point2D>{ new Point2D(0, -1), new Point2D(1,1) });

        // 16번 액세스
        //var pos = matchlist[15][0];
        //Debug.LogFormat($"{pos}");
    }

}


}
