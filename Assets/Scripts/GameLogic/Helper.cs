using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class Helper
{
    public static string GetStreamingAssetPath(string fileName)
    {
        var sbPath = new StringBuilder();

        // #if UNITY_ANDROID : 미리 정해져 있어 실행환경 결정이 필요.
        if(Application.platform == RuntimePlatform.Android)
        {
            sbPath.Append( $"jar:file://{Application.dataPath}!/assets/" );
        }
        else if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            sbPath.Append(Application.dataPath);
            sbPath.Append("/Raw/");
        }
        else if(Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.OSXEditor
            )
        {
            sbPath.Append(Application.dataPath);
            sbPath.Append("/StreamingAssets/");
        }
        else // 그 외의 환경은 미확인
        {

        }

        sbPath.Append(fileName);        
        return sbPath.ToString();
    }
}
