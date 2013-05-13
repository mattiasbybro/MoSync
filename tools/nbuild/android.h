#ifndef ANDROID_H
#define ANDROID_H

#include <map>
#include <string>

#include "args.h"

using namespace std;

int buildAndroidNative(Arguments* args);
int generateMakefile(Arguments* args, string& config);
int generateMakefiles(Arguments* params);
string toMakefileFile(string file);
string getNdkBuildScript(Arguments* params);
string getNdkBuildCommand(string commandLine);
string getAppPlatform(Arguments* params);
int executeNdkBuild(Arguments* args);
string getTempBuildDir(Arguments* params, string* cfg = NULL);
#ifdef WIN32
string getCygpath(bool& exists);
#endif

#endif