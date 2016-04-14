# Create a coverage report by:
# 1: using OpenCover to run the xunit tests
# 2: ReportGenerator for the HTML report (if COVERALLS_REPO_TOKEN NOT available)
# 3: Coveralls.net to export the generated OpenCover report to coveralls.io (if COVERALLS_REPO_TOKEN available)
if (Test-Path Env:APPVEYOR_PROJECT_NAME) {
	# Appveyor
	$projectName = $env:APPVEYOR_PROJECT_NAME
}
elseif (Test-Path Env:JOB_NAME){
	# Jenkins
	$projectName = $env:JOB_NAME
}
else {
	# Run from the working directory
	$projectName = Split-Path (pwd) -leaf
}
$filter="-filter:+`"[$projectName]*`""
$opencoverPath = ((gci packages\opencover* | sort-object name)[-1]).Fullname
$xunitrunnerPath = ((gci packages\xunit.runner.console* | sort-object name)[-1]).Fullname
$output="coverage.xml"

$openCoverArguments = @("-register:user", "$filter", "-target:$xunitrunnerPath\tools\xunit.console.exe","-targetargs:`"$projectName.Tests\bin\release\$projectName.Tests.dll -noshadow -xml xunit.xml`"","-output:`"$output`"")
Start-Process -wait $opencoverPath\tools\OpenCover.Console.exe -PassThru -NoNewWindow -ArgumentList $openCoverArguments

if (Test-Path Env:COVERALLS_REPO_TOKEN) {
	$coverallsPath = ((gci packages\\coveralls.io* | sort-object name)[-1]).Fullname
	$coverallsArguments = @("--opencover $output")
	Start-Process -wait $coverallsPath\tools\coveralls.net.exe -PassThru -NoNewWindow -ArgumentList $coverallsArguments
}
else {
	$reportgeneratorPath = ((gci packages\ReportGenerator* | sort-object name)[-1]).Fullname
	$reportgeneratorArguments = @("-reports:$output", "-targetdir:CoverageReport")
	Start-Process -wait $reportgeneratorPath\tools\ReportGenerator.exe -PassThru -NoNewWindow -ArgumentList $reportgeneratorArguments
}

