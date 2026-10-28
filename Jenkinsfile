
library 'ts-jenkins-shared-library@main'

pipeline {
    agent none
    options {
        copyArtifactPermission('*/TownSuite-Artifact-Publish')
        buildDiscarder(logRotator(numToKeepStr: '10'))
        timestamps()
        timeout(time: 2, unit: 'HOURS')
        skipDefaultCheckout true
    }
    environment {
        AUTOCONFIGURE_NUGET = 'true'
        AUTOCONFIGURE_DOCKER = 'true'
    }
    stages {
        stage('Start Automation Script') {
            agent { label 'starting-agent' }
            steps {
                script {
                    townsuite_automation2.start_linux()
                }
            }
        }    
        stage('Pipeline') {
            agent { label townsuite_automation2.get_linux_label() }
            stages {
                stage('Environment Setup') {
                    steps {
                        script {
                            townsuite.common_environment_configuration()
                            townsuite.checkout_scm()
                        }
                    }
                }
                stage('Build') {
                    steps {
                        sh '''
                        chmod +x buildrelease.ps1
                        ./buildrelease.ps1
                        '''
                    }
                }
                stage('Code Sign') {
                    when {
                        expression { return env.BRANCH_NAME.startsWith('PR-') == false }
                    }
                    steps {
                        echo 'Code Signing happening here....'
                        script {
                            townsuite.codesign "${env.WORKSPACE}", "*TownSuite*.dll;*TownSuite*.exe", false
                        }
                    }
                }
                stage('Nuget Package') {
                    steps {
                        sh '''
                        chmod +x build_nuget.ps1
                        ./build_nuget.ps1
                        '''
                    }
                }
                stage('Archive') {
                    steps {
                        echo 'archiving artifacts'
                        script {
                            townsuite.archiveWithRetryAndLock('build/*.nupkg,build/parameterproperties.txt', 3)
                        }
                    }
                }
            }
        }
    }
    post {
        always {
            CleanupVirtualMachines()
        }
        success {
            echo 'Pipeline executed successfully.'
        }
        failure {
            echo 'Pipeline failed.'
        }
        aborted {
            echo 'Pipeline was aborted.'
        }
    }
}

def CleanupVirtualMachines() {
    node('stopping-agent') {
        cleanWs()
        script {
            townsuite_automation2.stop_automation()
        }
    }
}