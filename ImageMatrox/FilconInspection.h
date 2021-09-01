#pragma once

#include "MatroxCommon.h"
#include <vector>

class CFilconInspection : public CMatroxCommon
{
public:
	CFilconInspection();
	~CFilconInspection();

	int execInspection(string nstrInputImage, string nstrResultImage);
};

