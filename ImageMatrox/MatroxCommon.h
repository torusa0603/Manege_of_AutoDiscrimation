//	Matrox基本クラス

#pragma once
#include <list>
#include "Global.h"

class CMatroxCommon
{
public:
	CMatroxCommon(void);
	virtual ~CMatroxCommon(void);

	//	マトロックスの基本的な初期化
	int Initial( HWND nhDispHandle, string nstrSettingPath );
	//	マトロックスの終了処理
	void CloseMatrox();
	//	指定した画像上のポイントの輝度値を取得する
	int getPixelValueOnPosition( POINT nNowPoint, int &RValue, int &GValue,int &BValue );
	//	デジタルズーム倍率を取得する
	double getZoomMag();
	//	指定のエリアの画像を保存する
	void saveImage( RECT nrctSaveArea, BOOL nAllSaveFlg, string ncsFilePath, BOOL nbSaveMono );
	//	今スルー中かフリーズ中か取得する
	bool getThoughStatus();
	//	フリーズ直後の画像に戻す
	void setOriginImage();
	//	表示バッファの倍率を設定する
	void setShowImageMag( double ndMag );
	//	画像サイズを取得する
	SIZE getImageSize();
	//	画像をロードする
	int	loadImage( string ncsFilePath );
	//	カラー設定を行う関数
	void	setColorImage( int niConversionType, int niPlane );
	//	接続されたカメラがカラーカメラかどうか調べる
	int getColorCameraInfo();
	//	差分用オリジナル画像を現在表示されている画像で登録する
	int setDiffOrgImage();
	//	差分モードを終了する
	void resetDiffMode();
	//	差分モードかどうか取得
	bool IsDiffMode(){ return m_bNowDiffMode; };

	//	オリジナル画像を保存する
	void saveOrigImage(RECT nrctSaveArea, BOOL nAllSaveFlg, string ncsFilePath );

	//	検査結果表示用ウインドウのハンドルをセットする
	int setDispHandleForInspectionResult( HWND nhDispHandleForInspectionResult );
	//	デジタルズーム倍率を取得する(検査結果表示用)
	double getZoomMagForInspectionResult();
	//	表示バッファの倍率を設定する(検査結果表示用)
	void setZoomMagForInspectionResult( double ndMag );
	//	検査結果画像を保存する
	void saveInspectionResultImage( string ncsFilePath );
	//	検査結果画像バッファをクリアする(描画もクリアする)
	void ClearInspectionResultImage();
	//	矩形領域内の平均輝度値を取得する
	int getAveragePixelValueOnRegion( RECT nrctRegion );
	//	カメラ映像が映るディスプレイを切り替える
	void SetDisplayHandle(HWND nhDispHandle);

	//	致命的なエラーが起きたか否か取得する
	bool getFatalErrorOccured();
	//	画像をファイルもしくはメモリ上から取得する
	void restoreImage(string nstrFileName, MIL_ID nmilSystem, MIL_ID *npmilImage);
	//	メモリ上に保存した画像を全てファイルに出力する
	void exportAllMemoryStoredImages();
	//	メモリ上に保存した画像1枚をファイルに保存し、画像リストから削除する
	void exportMemoryStoredImage(string nstrFileName);

	//	リングバッファ(m_lstImageGrab)の白黒画像データを取得する
	int getMonoBitmapData( const long nlArySize, BYTE *npbyteData );

protected:
	static MIL_ID	m_milApp;				//	アプリケーションID
	static MIL_ID	m_milSys;				//	システムID
	static MIL_ID	m_milDisp;				//	ディスプレイID
	static MIL_ID	m_milDigitizer;			//	デジタイザID
	static MIL_ID	m_milShowImage;			//	画像バッファ(表示用)
	static MIL_ID	m_milPreShowImage;		//	1つ前にグラブした画像バッファ
	static MIL_ID	m_milDiffOrgImage;		//	差分用オリジナル画像
	static MIL_ID	m_milDiffTargetImage;		//	差分用ターゲット画像
	static MIL_ID	m_milImageGrab[MAX_IMAGE_GRAB_NUM];	//	画像バッファ(グラブ専用)
	static MIL_ID	m_milAverageImageGrab[MAX_AVERAGE_IMAGE_GRAB_NUM];	//	平均化用画像バッファ(グラブ専用)
	static MIL_ID	m_milAverageImageCalc;	//	平均化用画像バッファ(積算用)
	static MIL_ID	m_milMonoImage;			//	1プレーンの画像(モノクロなら表示用バッファと同じ、カラーならばRGBのいずれか)
	static MIL_ID	m_milOriginalImage;		//	オリジナル画像(カラーバッファとして確保)
	static MIL_ID	m_milGraphic;			//	グラフィックバッファ
	static MIL_ID	m_milOverLay;			//	オーバーレイバッファ
	static string	m_strCameraFilePath;	//	DCFファイル名
	static SIZE		m_szImageSize;			//	画像サイズ
	static SIZE		m_szImageSizeForCamera;
	static int		m_iBoardType;			//	使用ボードタイプ
	static int		m_iNowColor;			//	現在のカラー
	static bool		m_bMainInitialFinished;
	static bool		m_bThroughFlg;			//	スルーならばTrue、フリーズならFalse
	static double	m_dNowMag;				//	現在の表示画像の倍率
	static HWND		m_hWnd;					//	ウインドウのハンドル
	static bool		m_bNowDiffMode;			//	現在差分表示モードか否か

	static HWND		m_hWndForInspectionResult;	//	ウインドウのハンドル(検査結果表示用)
	static MIL_ID	m_milDispForInspectionResult;				//	ディスプレイID(検査結果表示用)
	static MIL_ID	m_milInspectionResultImage;			//	画像バッファ(検査結果表示用)
	static MIL_ID	m_milOverLayForInspectionResult;			//	オーバーレイバッファ(検査結果表示用)
	static double	m_dNowMagForInspectionResult;				//	現在の検査結果表示画像の倍率
	static MIL_ID   m_milDraphicSaveImage;						//	グラフィックを画像に保存するためのバッファ

	static bool     m_bFatalErrorOccured;	//	致命的なエラー発生(ソフト再起動必須)

	static MIL_ID  m_milInspectionResultImageTemp;
	static double	m_dFrameRate;			//	カメラのフレームレート(FPS)
	static string  m_strIPAddress;			//  カメラのIPアドレス

	static int		m_iEachLightPatternMatching;	//照明毎にパターンマッチングを行うか否か　1:照明毎にパターンマッチング行う　0:一つの照明のみでパターンマッチング

	static std::list< MIL_ID >	m_lstImageGrab;		// ImageGrabのリスト

	//	画像バッファの再設定を実施する
	void reallocMilImage( SIZE nszNewImageSize );

	//	処理時間を出力
	void outputTimeLog(string nstrProcessName, double ndTime);

protected:
	//	エラーログ出力
	void outputErrorLog(string nstrErrorLog);
	void outputErrorLog(string nstrFunctionName, int niErrorCode);

	bool m_bDebugON;				//	デバッグ情報を出力する
	string m_strDebugFolder;		//	デバッグ情報出力フォルダー
	static string m_strDebugFileIdentifiedName;

private:
	//	パラメータファイルを読み込む
	void readParameter( string nstrSettingPath );
	//	エラーフック
	static long MFTYPE hookErrorHandler(long nlHookType, MIL_ID nEventId, void *npUserDataPtr);
	//	トリガで画像を取得処理(フック関数)
	static MIL_INT MFTYPE TriggerProcessingFunction( MIL_INT nHookType, MIL_ID nHookId, void* nHookDataPtr );
};
