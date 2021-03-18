pipeline {
  agent any
  stages {
    stage('Test') {
      steps {
        sh 'dotnet test --collect:"XPlat Code Coverage"'
        nunit testResultsPattern: '**/TestResults.xml'
        cobertura coberturaReportFile: '**/coverage.cobertura.xml'
      }
    }

    stage('Pack') {
      steps {
        sh 'find . -type f -name \\\'*.nupkg\\\' -delete'
        sh 'dotnet pack --version-suffix ${BUILD_NUMBER}'
      }
    }

    stage('Publish') {
      when {
        branch 'master'
      }
      steps {
        sh 'dotnet nuget push -s http://localhost:5000/v3/index.json **/*.nupkg -k $BAGET -n true'
      }
    }

  }
  environment {
    BAGET = credentials('3db850d0-e6b5-43d5-b607-d180f4eab676')
  }
}
