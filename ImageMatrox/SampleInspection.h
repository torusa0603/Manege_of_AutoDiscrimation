#pragma once

#include "MatroxCommon.h"
//#include <vector>

class CSampleInspection : public CMatroxCommon
{
public:
	CSampleInspection();
	~CSampleInspection();

	int execInspection( string nstrFolderName );

private:
	unsigned long get_msec( void );
	int write_bitmapfile( string nstrFilename, BYTE *npbyData, unsigned int nuiDataSize );
	int executeSoftwareTrigger( void );
};

