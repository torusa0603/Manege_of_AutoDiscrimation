import json

# Jsonファイルから読み込む関数
# 引数説明(設定ファイルパス、指定セクション名、情報を渡したい構造体)
def ReadElementsFromJsonfile(nstrPath, nstrSectionName, ndicElements):
    with open(nstrPath , 'r', encoding="utf-8") as m_jsonfile:
        m_dicElements_section = json.load(m_jsonfile)
        m_dicElements = m_dicElements_section[nstrSectionName]
    for loopkeys in ndicElements.keys():
        if loopkeys in m_dicElements:
            ndicElements[loopkeys] = m_dicElements[loopkeys]

# Jsonファイルに書き込む関数
# 引数説明(設定ファイルパス、指定セクション名、情報を渡したい構造体)
def WriteElementsToJsonfile(nstrPath ,nstrSectionName, ndicElements):
    with open(nstrPath , 'r', encoding="utf-8") as m_jsonfile:
        m_dicElements_section = json.load(m_jsonfile)
    with open(nstrPath , 'w', encoding="utf-8") as m_jsonfile:
        m_dicElements_section[nstrSectionName] = ndicElements
        m_dicElements = json.dumps(m_dicElements_section)
        m_jsonfile.write(m_dicElements)