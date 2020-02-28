pipeline {
  agent any
  stages {
    stage('Build') {
      steps {
        sh 'dotnet build **/Coroutine.csproj'
      }
    }

    stage('Pack') {
      steps {
        sh 'find . -type f -name \\\'*.nupkg\\\' -delete'
        sh 'dotnet pack **/Coroutine.csproj --version-suffix ${BUILD_NUMBER}'
      }
    }

    stage('Publish') {
      steps {
        sh '''dotnet nuget push -s http://localhost:5000/v3/index.json **/*.nupkg -k $BAGET -n true
'''
      }
    }

  }
  environment {
    BAGET = credentials('3db850d0-e6b5-43d5-b607-d180f4eab676')
  }
}