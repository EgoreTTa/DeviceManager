pipeline {
    agent any

    environment {
        DEPLOY_DIR  = '/home/ubuntu/API'		// Папка на цели
        SERVICE     = 'DeviceManager.service'	// Имя службы
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build & Publish API') {
            steps {
                sh 'sudo dotnet publish ./API/API.csproj -c Release -r linux-x64 --self-contained false -o ./Release/linux-x64'
            }
        }
		
        stage('Build & Publish drivers') {
            steps {
                sh '''
					set -e
					sudo mkdir -p ./Release/Drivers

					for dir in ./Drivers/*/; do
						[ -d "$dir" ] || continue
						if [ "$dir" = "DriverBase" ]; then
							echo "⏭️  Skipping $dir (shared/base library)"
							continue
						fi
						
						csproj=$(find "$dir" -maxdepth 1 -name "*.csproj" | head -n 1)
						[ -z "$csproj" ] && echo "⏭️  Skipping $dir (no .csproj)" && continue

						proj_name=$(basename "$dir")
						echo "🔨 Building $proj_name..."
						
						sudo dotnet publish "$csproj" -c Release -r linux-x64 -o "./Release/Drivers/$proj_name"
					done
				'''
            }
        }

		stage('Clean...') {
            steps {
				sh 'sudo rm -rf $DEPLOY_DIR/* && mkdir -p $DEPLOY_DIR/'
			}
		}
		stage('Move API') {
            steps {
				sh 'sudo mv -f ./Release/linux-x64/* $DEPLOY_DIR'
			}
		}
		stage('Move drivers') {
            steps {
				sh '''
					set -e
					sudo mkdir $DEPLOY_DIR/Drivers/
					
					for build_dir in ./Release/Drivers/*/; do
						[ -d "$build_dir" ] || continue
						
						dir_name=$(basename "$build_dir")
						dll_file="$build_dir/${dir_name}.dll"

						if [ -f "$dll_file" ]; then
							echo "📦 Deploying $dir_name.dll"
							sudo mv -f "$dll_file" "$DEPLOY_DIR/Drivers/$dir_name.dll"
						else
							echo "❌ Error: ${dir_name}.dll not found in $build_dir"
							exit 1
						fi
					done
				'''			}
		}
		stage('Restart Service') {
			steps {
				sh 'sudo chmod 777 $DEPLOY_DIR/ -R'
				sh 'sudo systemctl restart $SERVICE'
			}
		}
		stage('Get status Service') {
			steps {
				sh '''
					sleep 1
					
					for i in 1 2 3 4 5; do
						if sudo systemctl is-active --quiet $SERVICE; then
							echo "✅ Service is active (running)"
							exit 0
						fi
							echo "⏳ Waiting... ($i)"
						sleep 2
					done
					
					echo "❌ Service failed to start"
						sudo systemctl status $SERVICE --no-pager
						sudo journalctl -u $SERVICE --no-pager -n 20
					exit 1
				'''
			}
		}
    }
}