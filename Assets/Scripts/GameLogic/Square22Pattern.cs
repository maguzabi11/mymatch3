using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Match3
{
// 검사용 패턴 클래스
// 비슷한 클래스가 있어 추상화 필요하다

public class Square22Pattern : PatternBase
{
    private static Square22Pattern _intance;
    public static Square22Pattern Instance
    {
        get
        {
            if(null == _intance)
            {
                _intance = new Square22Pattern();
                _intance.Init();
            }

            return _intance;
        }
    }

    protected override void Init()
    {
        // '_'위치를 기준으로 상대적인 B(block)위치를 지정
        /*
         _B
         BB
         */
        matchlist.Add(new List<Point2D>{new Point2D(0,1), new Point2D(1,0), new Point2D(1,1)});

        /*
         B_
         BB
         */
         matchlist.Add(new List<Point2D>{new Point2D(0,-1), new Point2D(1,-1), new Point2D(1,0)});

        /*
         BB
         _B
         */
         matchlist.Add(new List<Point2D>{new Point2D(-1,0), new Point2D(-1,1), new Point2D(0,1)});

        /*
         BB
         B_
         */ 
         matchlist.Add(new List<Point2D>{new Point2D(-1,-1), new Point2D(-1,0), new Point2D(0,-1)});

         // 블록이 이동할 방항에 따라 검사할 인덱스를 지정할 수도 있다.
         // 많지 않으므로 필요 없다고 판단.
    }

}

}