@ECHO OFF
SET projectdir=%1
SET enginedir=%2

IF %projectdir%.==. (
	Echo * This script should be called from project folder 
	Echo * via "prepare_PROJECTNAME.cmd" script
	Echo * For instance, "prepare_cod.cmd" in COD folder
	Exit /B
)
echo Making links for project %projectdir%
:: Project name found, remove old link and make a new one
rd /s/q Assets\Scripts

mklink /j  Assets\NetworkEngine\  ..\%enginedir%\Assets\Scripts\


echo Done!
