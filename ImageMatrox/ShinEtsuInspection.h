#pragma once

#include "MatroxCommon.h"
#include <vector>

class CShinEtsuInspection:  public CMatroxCommon
{
public:
	CShinEtsuInspection();
	~CShinEtsuInspection();

	//	åüç∏é¿çs
	int execInspection(string nstrInputImage, string nstrResultImage, double ndMinDefectSize, int* npiNGContent, int* npiCurve);




};

