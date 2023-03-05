.PHONY: test
test:
	/Applications/Unity/Hub/Editor/2021.3.17f1/Unity.app/Contents/MacOS/Unity -runTests -projectPath . -batchmode -testPlatform EditMode -testResults ./TestResults/result.xml
