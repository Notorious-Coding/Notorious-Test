workspace "Notorious Test Framework" "Software architecture for the Notorious Test Framework." {

    !identifiers hierarchical

    model {
        u = person "Software Engineer"
        ntcore = softwareSystem "NotoriousTest Core" "Core of the framework."{
            environments = container "Environments" "Environments are used to manage the lifecycle of infrastructures."{
                base = component "BaseEnvironment" "Base environment is used to manage the lifecycle of infrastructures."
                configured = component "ConfiguredEnvironment" "Configured environment handle configuration transit between infrastructures."
            
                web = component "WebEnvironment" "Web Environment manage an API withing infrastructure's lifecycle."
            }

            infrastructures = container "Infrastructures" "Infrastructures initialize, reset and dispose a software like databases, message queues, web api, necessary for the tests."{

                group "Base"{
                    base = component "BaseInfrastructure" "Base infrastructure have the basics feature to initialize, reset and dispose a software."
                    configured = component "ConfiguredInfrastructure" "Configured infrastructure include configuration generation and consumption."                
                }

                group "Web"{
                    web = component "WebApplicationInfrastructure" "Web infrastructure create a inmemory API configured with the environment configuration."
                }
            }

            tests = container "Tests" "Tests are used to write the tests."{
                integration = component "IntegrationTests<Environment>" "Integration Tests use XUnit ClassFixtures to initialize, reset and dispose the environment."
            }

            webapp = container "WebApp" "WebApp is a web application used to test the framework."{
            }
        }

        nttestcontainers = softwareSystem "NotoriousTest.TestContainers" "Integrates testcontainers within infrastructures"{
            infrastructures = container "NotoriousTest.TestContainers.Infrastructures" "Infrastructures are used to manage the lifecycle of the tests."{
                docker = component "DockerInfrastructure" "Docker infrastructure is used to manage the lifecycle of the tests."
            }
        }

        ntsqlserver = softwareSystem "NotoriousTest.SqlServer" "Integrates SQL Server within infrastructures"{
            infrastructures = container "NotoriousTest.SqlServer.Infrastructures" "Infrastructures are used to manage the lifecycle of the tests."{
                sqlserver = component "SqlServerInfrastructure" "SqlServer infrastructure is used to create a SQL Server database."
            }
        }
        

        xunit = softwareSystem "xUnit"{
            tags "External"
            fixtures = container "Fixtures"{
                classfixtures = component "ClassFixtures" "Class fixtures are used to setup and teardown the test class."
            }
        }

        testcontainers = softwareSystem "testcontainers" "Tests containers is a library to manage docker containers within tests."{
            tags "External"
        }

        respawn = softwareSystem "Respawn" "Respawner is a library made to reset databases."{
            tags "External"
        }

        netcore = softwareSystem "NetCore" "NetCore is a framework to build web applications."{
            webappfactory = container "WebApplicationFactory" "WebApplicationFactory is a class to create a web application."
        }

        u -> ntcore.tests.integration "Write"

        // Tests
        ntcore.tests.integration -> xunit.fixtures.classfixtures "Integrates with"
        ntcore.tests.integration -> ntcore.environments.base "Manage"
        ntcore.tests.integration -> ntcore.environments.configured "Manage"
        ntcore.tests.integration -> ntcore.environments.web "Manage"

        // Environments
        ntcore.environments.base -> ntcore.infrastructures.base "Manage lifecycle of"
        ntcore.environments.configured -> ntcore.infrastructures.configured "Manage lifecycle and transit configuration"
        ntcore.environments.web -> ntcore.infrastructures.web "Start API at the end of the test campaign with"

        ntcore.environments.configured -> ntcore.environments.base "Inherit from"
        ntcore.environments.web -> ntcore.environments.configured "Inherit from"
        
        // Infrastructures
        ntcore.infrastructures.configured -> ntcore.infrastructures.base "Inherit from"
        ntsqlserver.infrastructures.sqlserver -> ntcore.infrastructures.configured  "Inherit from"
        ntsqlserver.infrastructures.sqlserver -> respawn "Reset database with"
        ntsqlserver.infrastructures.sqlserver -> nttestcontainers.infrastructures.docker "Start and stop SQL Server with"
        nttestcontainers.infrastructures.docker -> ntcore.infrastructures.configured "Inherit from"
        nttestcontainers.infrastructures.docker -> testcontainers "Manage docker containers with"
        ntcore.infrastructures.web -> ntcore.infrastructures.configured "Inherit from"
        ntcore.infrastructures.web -> ntcore.webapp "Configure and Start API with"

        ntcore.webapp -> netcore.webappfactory "Create web application with"
    }

    views {
        systemContext ntcore "Diagram1" {
            include *
        }

        container ntcore "Diagram2" {
            include u
            include xunit.fixtures
            include respawn
            include testcontainers
            include ntcore.environments
            include ntcore.infrastructures
            include ntcore.tests
            include nttestcontainers.infrastructures
            include ntsqlserver.infrastructures
            
        }

        component ntcore.infrastructures "Diagram3" {
            include u
            include xunit.fixtures.classfixtures
            include ntcore.environments.base
            include ntcore.environments.configured
            include ntcore.environments.web
            include ntcore.infrastructures.base
            include ntcore.infrastructures.configured
            include ntcore.infrastructures.web
            include nttestcontainers.infrastructures.docker
            include ntcore.tests.integration
            include ntsqlserver.infrastructures.sqlserver
            include respawn
            include testcontainers
            include ntcore.webapp
            include netcore.webappfactory
        }

        styles {
            element "Element" {
                color #000000
                background #ba1e25
            }
            element "Person" {
                background #9b191f
                shape person
            }
            element "Software System" {
                background #ba1e25
            }
            element "Container" {
                background #d9232b
            }
            element "Database" {
                shape cylinder
            }
        }
    }
}