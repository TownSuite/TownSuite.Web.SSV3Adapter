
library 'ts-jenkins-shared-library@main'

// main and release branches start at most one build an hour instead of one per push: a push
// that arrives inside the hour waits in the queue, later pushes fold into that one build, and
// it builds the newest commit when the hour is up. A build started by hand is not held back
// (userBoost). Pull requests and other branches build on every push; the limit there is set
// too high to ever apply. TownSuite/TownSuite.IssueTracker#17881.
int builds_per_hour = townsuite.is_publishing_branch() ? 1 : 1000

pipeline {
    agent none
    options {
        rateLimitBuilds(throttle: [count: builds_per_hour, durationName: 'hour', userBoost: true])
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