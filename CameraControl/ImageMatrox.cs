using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;

using ImageMatrox;
using System.Reflection;


namespace CameraControl
{
	/// <summary>
	/// ImageMatrox.dllのラッパークラス
	/// </summary>
	public class CImageMatrox : extern_main
    {
        //public extern_main ImageMatroxDllMethod;
        //public CImageMatrox()
        //{
        //    ImageMatroxDllMethod = new extern_main();
        //}

        #region DLLインポート
        //      [DllImport( "ImageMatrox.dll", EntryPoint = "sifInitializeImageProcess") ]
        //public static extern int sifInitializeImageProcess( IntPtr nhDispHandle, string nstrSettingPath = null );

        //[ DllImport( "ImageMatrox.dll" ) ]
        //public static extern void sifCloseImageProcess();

        //[ DllImport( "ImageMatrox.dll" ) ]
        //public static extern int sifThrough();

        //[ DllImport( "ImageMatrox.dll" ) ]
        //public static extern void sifGetOneGrab();

        //[ DllImport( "ImageMatrox.dll" ) ]
        //public static extern void sifSaveImage( tag_rect nrctSaveArea, bool nAllSaveFlg, string nstrFilePath, bool nbSaveMono );

        //[ DllImport( "ImageMatrox.dll" ) ]
        //public static extern int sifSetDispHandleForInspectionResult( IntPtr nhDispHandle );

        //      [DllImport("ImageMatrox.dll")]
        //      public static extern Size sifGetImageSize();

        //[ DllImport( "ImageMatrox.dll" ) ]
        //public static extern int sifSetTriggerModeOff();

        //[ DllImport( "ImageMatrox.dll" ) ]
        //public static extern int sifSetTriggerModeSoftware();

        //[ DllImport( "ImageMatrox.dll" ) ]
        //public static extern int sifSetTriggerModeHardware( string nstrTrigger );

        //[ DllImport( "ImageMatrox.dll" ) ]
        //public static extern int sifExecuteSoftwareTrigger();

        //[ DllImport( "ImageMatrox.dll" ) ]
        //public unsafe static extern int sifGetMonoBitmapData( long nlArySize, byte *npbyteData );

        //[ DllImport( "ImageMatrox.dll" ) ]
        //public static extern int sifSampleInspection( string nstrFolderName );
        #endregion
    }
}
