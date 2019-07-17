using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  Match3
{
    using MatchList = List<Point2D>;
    
public class ScoreManager 
{
    int _score;

    // 점수 상한 필요. 

    public int Score
    {
        get {return _score;}
    }

    public void Init()
    {
        _score = 0;
    }

    public int Calculate( List<MatchInfo> matches )
    {
        int pointBase = 10;
        int sum = 0;

        for(int i=0; i<matches.Count; ++i)
        {
            sum += matches[i].matchlist.Count * pointBase;
            pointBase += 5;
        }

        _score += sum;
        return sum;
    }

}


}
