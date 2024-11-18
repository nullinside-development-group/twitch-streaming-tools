pipeline {
    agent any
	options {
        ansiColor('xterm')
    }

    stages {
        stage('Checkout') {
            steps {
                git branch: env.BRANCH_NAME, credentialsId: 'GitHub PAT', url: 'https://github.com/[GitOwnerAndRepoName].git'
            }
        }
        
        stage('Build & Deploy') {
            steps {
				withCredentials([
					string(credentialsId: 'GITHUB_NULLINSIDE_ORG_RELEASE_TOKEN', variable: 'GITHUB_NULLINSIDE_ORG_RELEASE_TOKEN')
				]) {
					sh """
						bash go.sh 
					"""
				}
            }
        }
    }
	
	post {
		always {
			cleanWs cleanWhenFailure: false, notFailBuild: true
		}
	}
}
