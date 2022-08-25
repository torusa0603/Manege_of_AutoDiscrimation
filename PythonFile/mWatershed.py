import cv2
import numpy as np
import glob
from matplotlib import pyplot as plt
import cHandleJsonfile
import os
import math
from enum import Enum

# 設定ファイルから読み込む変数群を格納した辞書型変数 
m_dicSettingElements={"NumberPerOneMiliMeter" : "", "MaxRadius" : "","circle_level_threshold":"","white_satuation":""} 

# 表示時タイプ
class ShowMode(Enum):
  Image = 0
  Plot = 1

#-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------#
### メイン関数
# 引数(表示フラグ, C#作成アプリでの使用を目的としたためそのアプリの存在ディレクトリ, 詳細フラグ, 解析するファイル名を限定するかのフラグ)
def main(nbShowFlag, nstrExeFolderPath, nbDetail, nbLimitFileName):
  if nstrExeFolderPath == "":
    nstrExeFolderPath = "."
  # 設定ファイルを読み込む
  cHandleJsonfile.ReadElementsFromJsonfile(os.path.join(nstrExeFolderPath,"Setting.json"), 'mWaterShed', m_dicSettingElements)
  str_file_name = ""
  if nbLimitFileName:
    # img.pngというファイル名のみ解析する
    # アプリに組み込む際は限定しておいた方が余計なファイルに惑わされない
    str_file_name = "img/img.png"
  else:
    # PNGファイルなら何でも解析する
    str_file_name = "img/*.png"
  for fn in glob.glob(os.path.join(nstrExeFolderPath, str_file_name)):
    # 画像の読み込み
    img_raw = cv2.imread(fn)
    showPicture(img_raw, 'Sorce', ShowMode.Image, nbShowFlag)
    #BGR→HSV変換
    img_hsv = cv2.cvtColor(img_raw, cv2.COLOR_BGR2HSV)
    # 色相の値が緑であるかで画像を二値化する
    arr_lower_1 = (40, 30, 30)
    arr_upper_1 = (80, 255, 255)
    img_bind_green = cv2.inRange(img_hsv, arr_lower_1, arr_upper_1, 255)
    showPicture(img_bind_green, 'bin_img_green', ShowMode.Image, nbShowFlag)
    # 明度の高低で画像を二値化する
    arr_lower_2 = (0, 0,  40)
    arr_upper_2 = (179, 255, 255)
    img_bind_white = cv2.inRange(img_hsv, arr_lower_2, arr_upper_2, 255)
    showPicture(img_bind_white, 'bin_img_white', ShowMode.Image, nbShowFlag)
    # 彩度が高いor明度が高い部分を白、その他の部分を黒しとした二値画像を作成
    img_bind_mask = cv2.add(img_bind_green, img_bind_white)
    showPicture(img_bind_mask, 'bin_img', ShowMode.Image, nbShowFlag)
    # デッドロック対策のカウンターをセット
    i_count = 1
    # WaterShedを行う
    b_ret, i_number_labels, arr_center,  i_number_of_color_and_radius, npdata_number_of_color_and_radius = doWatershedMethod(img_raw, img_bind_mask, nbShowFlag, nbDetail, i_count)
    if b_ret:
      if nbDetail:
        color_band = ["R","Y","G","B","V","W"]
        for i in range(i_number_labels):
          cv2.putText(img_raw, "ID: " +str(i + 1),                                      ((int)(arr_center[i][0] - 40),(int)(arr_center[i][1] + 30)), cv2.FONT_HERSHEY_PLAIN, 2, (255, 0, 0))
          cv2.putText(img_raw, "Color: " + color_band[npdata_number_of_color_and_radius[i][0]], ((int)(arr_center[i][0] - 40),(int)(arr_center[i][1] + 60)), cv2.FONT_HERSHEY_PLAIN, 2, (255, 0, 0))
          cv2.putText(img_raw, "Size: " +str(npdata_number_of_color_and_radius[i][1]),  ((int)(arr_center[i][0] - 40),(int)(arr_center[i][1] + 90)), cv2.FONT_HERSHEY_PLAIN, 2, (255, 0, 0))
          cv2.imwrite(os.path.join(nstrExeFolderPath, "img/result.png"), img_raw)
      showPicture(img_raw, 'img_marked', ShowMode.Image, nbShowFlag)
    else:
      # 0埋めのリストを作成する
      i_number_of_color_and_radius = np.zeros([6, m_dicSettingElements["MaxRadius"]], dtype=np.int)
    np.savetxt(os.path.join(nstrExeFolderPath, "result/color_radius.csv"), i_number_of_color_and_radius, delimiter=",", fmt="%d")
#-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------#


#-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------#
### WaterShedを行う
# 引数(元画像, マスク画像, 画像表示フラグ, 詳細フラグ_デバッグ時に結果を保存したりするために使用する, 再帰呼び出しのデッドロック防止用の呼出回数カウント)
# 返り値(返り値に中身が入っているかのフラグ, ブロブ個数, 中心座標, (色, サイズ)毎の個数を表す行列, 各オブジェクトの(色, サイズ)の値)
def doWatershedMethod(nimgRaw, nimgBindMask, nbShowFlag, nbDetail, niCount):
  #デッドロック対策
  niCount += 1
  #回帰関数を10回繰り返した場合に終了する
  if niCount == 10:
    return False, None, None, None, None
  # オープニング処理
  i_kernel_5_5 = np.ones((5,5),np.uint8)
  img_opening = cv2.morphologyEx(nimgBindMask,cv2.MORPH_OPEN,i_kernel_5_5,iterations = 2)
  # クロージング処理
  i_kernel_3_3 = np.ones((3,3),np.uint8)
  img_opening_closing = cv2.morphologyEx(img_opening, cv2.MORPH_CLOSE, i_kernel_3_3,iterations = 5)
  showPicture(img_opening_closing, 'opening_closing', ShowMode.Image, nbShowFlag)
  #白色ピクセルの個数が少なければ、終了
  img_closing_5_5 = cv2.morphologyEx(img_opening, cv2.MORPH_CLOSE, i_kernel_5_5,iterations = 5)
  showPicture(img_closing_5_5, 'img_closing_5_5', ShowMode.Image, nbShowFlag)
  # 画像内の白ビットが少ない(ノイズらが残っているだけ)状態になったら終了
  count_white_bit = cv2.countNonZero(img_closing_5_5)
  if (count_white_bit < 5000):
    return False, None, None, None, None
  # 明確な背景を抽出
  img_sure_bg = cv2.dilate(img_opening_closing, i_kernel_3_3, iterations=2)
  showPicture(img_sure_bg, 'sure_bg', ShowMode.Image, nbShowFlag)
  #距離変換処理
  img_dist_transform = cv2.distanceTransform(img_opening_closing, cv2.DIST_L2, 5)
  showPicture(img_dist_transform, '', ShowMode.Plot, nbShowFlag)
  # 明確な前景を抽出
  _, img_sure_fg = cv2.threshold(img_dist_transform, 0.5*img_dist_transform.max(), 255, 0)
  showPicture(img_sure_fg, '', ShowMode.Plot, nbShowFlag)
  # 分割したい領域を抽出
  img_sure_fg = np.uint8(img_sure_fg)
  img_unknown = cv2.subtract(img_sure_bg, img_sure_fg)
  showPicture(img_unknown, '', ShowMode.Plot, nbShowFlag)
  # オブジェクトごとに番号を振っていく
  if nbDetail:
    i_number_labels_high, arr_markers, data_high, center_high = cv2.connectedComponentsWithStats(img_sure_fg)
    center_high = center_high[1 : (i_number_labels_high + 1), : ]
  else:
    i_number_labels_high, arr_markers = cv2.connectedComponents(img_sure_fg)
    center_high = "None"
  showPicture(arr_markers, '', ShowMode.Plot, nbShowFlag)
  # 前景(番号ごとに代入する値を変更している)・背景・分割したい領域をそれぞれ明確にする
  arr_markers = arr_markers + 1
  arr_markers[img_unknown == 255] = 0
  showPicture(arr_markers, '', ShowMode.Plot, nbShowFlag)
  # 明確な前景・背景・分割したい領域が明確になったのでWaterShedを行う
  img_opening_closing_bgr = cv2.cvtColor(img_opening_closing, cv2.COLOR_GRAY2BGR)
  markers_watershed = cv2.watershed(img_opening_closing_bgr, arr_markers)
  # 背景部分には1を代入
  markers_watershed[markers_watershed == -1] = 1
  showPicture(markers_watershed, '', ShowMode.Plot, nbShowFlag)
  # 番号付けしたオブジェクト群のパラメータを取得する
  i_number_of_color_and_radius_high, npdata_number_of_color_and_radius_high, i_number_labels_high, center_high = DistincteLabels(nimgRaw, i_number_labels_high, markers_watershed, nbShowFlag, nbDetail, center_high)
  #nimgRaw[markers_watershed == -1] = [0,255,0]
  # 番号付けが終了したビットは0埋めする
  img_opening_closing[markers_watershed != 1] = 0
  showPicture(img_opening_closing, 'opening_closing_remove', ShowMode.Image, nbShowFlag)
  # オブジェクトのサイズが異なる場合は小さい物は無視されてしまう恐れがあるので回帰させる
  b_ret, i_number_labels_low, center_low,  i_number_of_color_and_radius_low, npdata_number_of_color_and_radius_low = doWatershedMethod(nimgRaw, img_opening_closing, nbShowFlag, nbDetail, niCount)
  # 下レイヤーの関数でもWaterShedを行った場合はb_retにTrueが入ってくるので、このレイヤの返り値に追加を行う
  if b_ret:
    i_number_of_color_and_radius_high += i_number_of_color_and_radius_low
    i_number_labels_high += i_number_labels_low
    npdata_number_of_color_and_radius_high = np.append(npdata_number_of_color_and_radius_high, npdata_number_of_color_and_radius_low, axis=0)
    if nbDetail:
      center_high = np.append(center_high, center_low, axis=0)
  return True, i_number_labels_high, center_high, i_number_of_color_and_radius_high, npdata_number_of_color_and_radius_high
#-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------#


#-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------#
### 円形度を計算する
# 引数(ラベル付け済みイメージ, 解析対象ラベル, 表示フラグ)
# 返し値(円らしさ値)
def calcCircularity(nlsLabelTable, niLabel, nbShowFlag):
  nlsLabelTableCopy = nlsLabelTable.copy()
  nlsLabelTableCopy[nlsLabelTable != niLabel] = 0
  nlsLabelTableCopy[nlsLabelTable == niLabel] = 1
  showPicture(nlsLabelTableCopy, '', ShowMode.Plot, nbShowFlag)
  contour , hierarchy = cv2.findContours(nlsLabelTableCopy, cv2.RETR_CCOMP, cv2.CHAIN_APPROX_SIMPLE)
  if len(contour) == 0:
    #円周がとれないのなら0とする
    return 0
  perimeter = cv2.arcLength(contour[0], True)
  area = cv2.contourArea(contour[0])
  # (オブジェクトの総ビット数)/(円周から計算される円の面積)の割合から"円らしさ"を計算
  i_circle_level = (int)((4.0 * np.pi *area / (perimeter * perimeter) )* 100)
  return i_circle_level
#-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------#

#-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------#
### イメージを表示させる
def showPicture(nimgPicture, nstrTitle, neMode, nbShow):
  if(nbShow):
    if(neMode == ShowMode.Image):
      # 画像高さが1080pix以下になるようにリサイズする
      i_height, i_width = nimgPicture.shape[:2]
      i_ratio = 1
      if (i_height / i_ratio) > 1080:
          i_ratio += 1
      img_resize_picture = cv2.resize(nimgPicture, dsize=(int(i_width / i_ratio), int(i_height / i_ratio)))
      # 画像を出力する
      cv2.imshow(nstrTitle,img_resize_picture)
      # qボタンが押されたら終了する
      key = ord('a')
      while key != ord('q'):
        key = cv2.waitKey(1)
      # ウィンドウ情報を消去する
      cv2.destroyAllWindows()
    elif(neMode == ShowMode.Plot):
      plt.imshow(nimgPicture)
      plt.show()
      plt.clf()
    else:
      pass
#-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------#


#-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------#
### 各ラベルを判別させる
# 引数(元画像, ラベル個数, ラベル付け済みイメージ, 表示フラグ, 詳細フラグ, 中心座標) 
# 返り値((色, サイズ)毎の個数を表す行列, 各オブジェクトの(色, サイズ)の値, オブジェクト個数, 中心座標)
def DistincteLabels(nimgSrc, niLabelNumber, nlsLabelTable, nbShowFlag, nbDetail, nCenterPoint):
  i_number_of_color_and_radius = np.zeros([6, m_dicSettingElements["MaxRadius"]], dtype=np.int) #(赤,黄,緑,青,紫,白)ごとの個数
  i_label_number = niLabelNumber
  # 初めに配列を作り入れておく、後で消す(この処理をしないとなぜかエラーが発生するのでいれているので消さないこと)
  npdata_number_of_color_and_radius = np.array([[0, 0]]) # 各オブジェクトの(色,サイズ)情報
  for label in range(2, niLabelNumber + 1):
    # 円らしさを計算する
    i_circle_level = calcCircularity(nlsLabelTable, label, nbShowFlag)
    # 円らしさオブジェクト処理を分ける
    if (i_circle_level >= m_dicSettingElements["circle_level_threshold"]):
      i_label_group_index = np.where(nlsLabelTable == label) #現ラベル数のブロブ情報
      i_array_label_bgr = nimgSrc[i_label_group_index] #rgb情報
      i_width = i_array_label_bgr.shape[0] #所属ブロブ個数
      # 半径を決定
      i_ret_radius = CalculateRadius(i_width, m_dicSettingElements["MaxRadius"], m_dicSettingElements["NumberPerOneMiliMeter"])
      # メディアンフィルター
      img_label_bgr_median = cv2.medianBlur(i_array_label_bgr,5)
      img_label_bgr = np.zeros((1, i_width, 3), dtype='uint8') #rgb情報を格納するnumpy型変数
      # opencvのメソッドを使用するために一列の長い画像として情報を格納する
      img_label_bgr[0, :, :] = img_label_bgr_median 
      # rgb→hsv変換
      img_hsv = cv2.cvtColor(img_label_bgr, cv2.COLOR_BGR2HSV)
      # hsvそれぞれの平均値を算出
      hsv_list =  list(cv2.split(img_hsv))
      hsv_without_white = [hsv_list[0][0][i] for i in range(hsv_list[0].shape[1]) if (hsv_list[1][0][i] > m_dicSettingElements["white_satuation"])]
      # 明度が高く・彩度が低いブロブを白色とし、その条件外のブロブはhueを使用して色を決定させる
      white_ratio = (hsv_list[0].shape[1]-len(hsv_without_white)) / hsv_list[0].shape[1]
      if white_ratio > 0.9:
        i_ret_color = 5
      else:
        i_h_mean = sum(hsv_without_white) / len(hsv_without_white)
        i_ret_color = DistincteColor(i_h_mean)
      # 該当する(色, サイズ)の要素を増やす
      i_number_of_color_and_radius[i_ret_color][i_ret_radius - 1] += 1
      # 各オブジェクト毎の(色, サイズ)を追加する
      npdata_number_of_color_and_radius = np.append(npdata_number_of_color_and_radius, np.array([np.array([i_ret_color, i_ret_radius - 1])]), axis=0)
    else:
      # 円らしくないオブジェクトは削除する
      if nbDetail:
        nCenterPoint = np.delete(nCenterPoint, label - 2 - (niLabelNumber - i_label_number), axis = 0)
      i_label_number -= 1
  # 初めに作成した配列(一番前)を消去する
  npdata_number_of_color_and_radius = npdata_number_of_color_and_radius[1 : len(npdata_number_of_color_and_radius), : ]
  # 背景部もオブジェクトの一つとして入力されてくるので減らしておく
  i_label_number -= 1
  # 色, 半径)それぞれの個数を配列として返す
  return i_number_of_color_and_radius, npdata_number_of_color_and_radius, i_label_number, nCenterPoint
#-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------#


#-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------#
### 色を判別させる
# 引数(色相値)
# 返し値(対応番号)
def DistincteColor(niHue):
  # hueの値を(赤,黄,緑,青,紫)に区分けする為の区切り数値
  i_color_band = (10,50,100,140,160)
  # 色判定：赤
  if i_color_band[0] > niHue or niHue >= i_color_band[4]:
    i_ret = 0
  # 色判定：黄
  elif i_color_band[0] <= niHue < i_color_band[1]:
    i_ret = 1
  # 色判定：緑
  elif i_color_band[1] <= niHue < i_color_band[2]:
    i_ret = 2
  # 色判定：青
  elif i_color_band[2] <= niHue < i_color_band[3]:
    i_ret = 3
  # 色判定：紫
  elif i_color_band[3] <= niHue < i_color_band[4]:
    i_ret = 4
  # hueを判定した色の要素のみに1を入れて返す
  return i_ret
#-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------#


#-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------#
### 半径を測定する
# 引数(オブジェクト構成ビット数, 最大半径, ピクセルサイズ)
# 返し値(半径値)
def CalculateRadius(niNamberOfPixels, niMaxRadius, ndNumberPerOneMiliMeter):
  #半径毎の面積ピクセル数を計算し、比較する
  for i_radius in range(niMaxRadius):
    i_area = math.pi * (i_radius + 1) ** 2 * ndNumberPerOneMiliMeter**2
    # 計算した面積ピクセル数がブロブのピクセル数よりも大きくなった時にブレイク
    if niNamberOfPixels < i_area:
      break
  # 半径を返す
  return i_radius
#-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------#



if __name__ == '__main__':
  main(True, "", True, False)