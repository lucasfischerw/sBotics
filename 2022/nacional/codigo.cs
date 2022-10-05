int[][] AngulosRetos = {new []{0, 90, 180, 270, 360}, new []{0, 45, 90, 135, 180, 225, 270, 315, 360}};
int VarTempo = 0;
int VarTempoDir = 0;
int VarTempoEsq = 0;
int VarTempoTendencioso = -89;
float ValorTendencioso = 0;
bool DesvioVerdadeiro = false;

float[] ValoresLuz = new[]{0f, 0f};

async Task<int> Angulo(int Precisao) {
	float Direcao = (float)Bot.Compass;
	var MaisPerto = int.MaxValue;
	var DiferencaMinima = int.MaxValue;
	foreach (var Numero in AngulosRetos[Precisao]) {
		var Diferenca = Math.Abs((long)Numero - Direcao);
		if (DiferencaMinima > Diferenca) {
			DiferencaMinima = (int)Diferenca;
			MaisPerto = Numero;
		}
	}
	return MaisPerto;
}

async Task<float> Diferenca(float Precisao) {
    return (float)(-Bot.Compass+(await Angulo((int)Precisao)));
}

float Luz(int Sensor) {
    if(Sensor == 1) {
        return (float)Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness;
    } else {
        return (float)Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness;
    }
}

async Task<int> Objetivo(int AnguloDesejado, int Precisao) {
	int AnguloAtual = await Angulo(Precisao);
	if(AnguloDesejado >= 0) {
		return ((AnguloAtual+AnguloDesejado)-4)%360;
	} else {
		if(((AnguloAtual+AnguloDesejado)+4)%360 >= 0) {
			return ((AnguloAtual+AnguloDesejado)+4)%360;
		} else {
			return 360+((AnguloAtual+AnguloDesejado)+4);
		}
	}
}

async Task Alinhar() {
    IO.Print((await Angulo(0)-Bot.Compass).ToString());
    if(await Angulo(0)-Bot.Compass > 0.5) {
        while(await Angulo(0)-Bot.Compass > 3) {
            await GirarDireita(200);
            await Time.Delay(16);
        }
        while(await Angulo(0)-Bot.Compass > 0.5) {
            await GirarDireita(50);
            await Time.Delay(16);
        }
    } else if(await Angulo(0)-Bot.Compass < -0.5) {
        while(await Angulo(0)-Bot.Compass < -3) {
            await GirarEsquerda(200);
            await Time.Delay(16);
        }
        while(await Angulo(0)-Bot.Compass < -0.5) {
            await GirarEsquerda(50);
            await Time.Delay(16);
        }
    }
}

async Task Destravar() {
    Bot.GetComponent<Servomotor>("MotorDireitaFrente").Locked = false;
    Bot.GetComponent<Servomotor>("MotorEsquerdaFrente").Locked = false;
    Bot.GetComponent<Servomotor>("MotorDireitaTras").Locked = false;
    Bot.GetComponent<Servomotor>("MotorEsquerdaTras").Locked = false;
}

async Task Parar() {
    Bot.GetComponent<Servomotor>("MotorDireitaFrente").Locked = true;
    Bot.GetComponent<Servomotor>("MotorEsquerdaFrente").Locked = true;
    Bot.GetComponent<Servomotor>("MotorDireitaTras").Locked = true;
    Bot.GetComponent<Servomotor>("MotorEsquerdaTras").Locked = true;
}

async Task Frente(int Velocidade) {
    Bot.GetComponent<Servomotor>("MotorDireitaFrente").Apply(50, Velocidade);
    Bot.GetComponent<Servomotor>("MotorEsquerdaFrente").Apply(50, Velocidade);
    Bot.GetComponent<Servomotor>("MotorDireitaTras").Apply(50, Velocidade);
    Bot.GetComponent<Servomotor>("MotorEsquerdaTras").Apply(50, Velocidade);
}

async Task Tras(int Velocidade) {
    Bot.GetComponent<Servomotor>("MotorDireitaFrente").Apply(50, -Velocidade);
    Bot.GetComponent<Servomotor>("MotorEsquerdaFrente").Apply(50, -Velocidade);
    Bot.GetComponent<Servomotor>("MotorDireitaTras").Apply(50, -Velocidade);
    Bot.GetComponent<Servomotor>("MotorEsquerdaTras").Apply(50, -Velocidade);
}

async Task GirarDireita(int Velocidade) {
    Bot.GetComponent<Servomotor>("MotorDireitaFrente").Apply(50, -Velocidade);
    Bot.GetComponent<Servomotor>("MotorEsquerdaFrente").Apply(50, Velocidade);
    Bot.GetComponent<Servomotor>("MotorDireitaTras").Apply(50, -Velocidade);
    Bot.GetComponent<Servomotor>("MotorEsquerdaTras").Apply(50, Velocidade);
}

async Task GirarEsquerda(int Velocidade) {
    Bot.GetComponent<Servomotor>("MotorDireitaFrente").Apply(50, Velocidade);
    Bot.GetComponent<Servomotor>("MotorEsquerdaFrente").Apply(50, -Velocidade);
    Bot.GetComponent<Servomotor>("MotorDireitaTras").Apply(50, Velocidade);
    Bot.GetComponent<Servomotor>("MotorEsquerdaTras").Apply(50, -Velocidade);
}

async Task GirarMotores(float ForcaDireita, float ForcaEsquerda) {
    Bot.GetComponent<Servomotor>("MotorDireitaFrente").Apply(150, ForcaDireita);
    Bot.GetComponent<Servomotor>("MotorEsquerdaFrente").Apply(150, ForcaEsquerda);
    Bot.GetComponent<Servomotor>("MotorDireitaTras").Apply(150, ForcaDireita);
    Bot.GetComponent<Servomotor>("MotorEsquerdaTras").Apply(150, ForcaEsquerda);
}

async Task FecharGarra() {
    int Velocidade = 250;
    int DireitaAberta = 0;
    int EsquerdaAberta = -90;
    int DireitaFechada = -20;
    int EsquerdaFechada = -70;

    Bot.GetComponent<Servomotor>("GarraDireita").Locked = false;
    Bot.GetComponent<Servomotor>("GarraEsquerda").Locked = false;
    Bot.GetComponent<Servomotor>("GarraDireita").Apply(500, -Velocidade);
    Bot.GetComponent<Servomotor>("GarraEsquerda").Apply(500, Velocidade);
    await Time.Delay(200);
    // int TempoInicio = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    // while(Bot.GetComponent<Servomotor>("GarraEsquerda").Angle < EsquerdaFechada && Bot.GetComponent<Servomotor>("GarraDireita").Angle > DireitaFechada) {
    //     // IO.Print((Bot.GetComponent<Servomotor>("GarraEsquerda").Angle).ToString());
    //     IO.Print("Loop");
    //     Bot.GetComponent<Servomotor>("GarraDireita").Apply(500, -Velocidade);
    //     Bot.GetComponent<Servomotor>("GarraEsquerda").Apply(500, Velocidade);
    //     await Time.Delay(16);   
    //     if((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-TempoInicio > 100) {
    //         Bot.GetComponent<Servomotor>("GarraDireita").Locked = true;
    //         Bot.GetComponent<Servomotor>("GarraEsquerda").Locked = true;
    //         await Time.Delay(100);
    //         Bot.GetComponent<Servomotor>("GarraDireita").Locked = false;
    //         Bot.GetComponent<Servomotor>("GarraEsquerda").Locked = false;
    //         await Time.Delay(100);
    //         TempoInicio = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    //     } 
    // }
    // IO.Print("sai");
    // while(Bot.GetComponent<Servomotor>("GarraEsquerda").Angle < EsquerdaFechada) {
    // // IO.Print((Bot.GetComponent<Servomotor>("GarraEsquerda").Angle).ToString());
    //     Bot.GetComponent<Servomotor>("GarraEsquerda").Apply(500, Velocidade);
    //     await Time.Delay(16);   
    //     if((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-TempoInicio > 100) {
    //         Bot.GetComponent<Servomotor>("GarraEsquerda").Locked = true;
    //         Bot.GetComponent<Servomotor>("GarraEsquerda").Locked = false;
    //         await Time.Delay(100);
    //         TempoInicio = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    //     }    
    // }
    // while(Bot.GetComponent<Servomotor>("GarraDireita").Angle > DireitaFechada) {
    //     Bot.GetComponent<Servomotor>("GarraDireita").Apply(500, -Velocidade);
    //     await Time.Delay(16);   
    //     if((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-TempoInicio > 100) {
    //         Bot.GetComponent<Servomotor>("GarraDireita").Locked = true;
    //         Bot.GetComponent<Servomotor>("GarraDireita").Locked = false;
    //         await Time.Delay(100);
    //         TempoInicio = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    //     }   
    // }
    Bot.GetComponent<Servomotor>("GarraDireita").Locked = true;
    Bot.GetComponent<Servomotor>("GarraEsquerda").Locked = true;
}

async Task AbrirGarra() {
    int Velocidade = 200;
    int DireitaAberta = 180;
    int EsquerdaAberta = 90;
    int DireitaFechada = 210;
    int EsquerdaFechada = 120;

    Bot.GetComponent<Servomotor>("GarraDireita").Locked = false;
    Bot.GetComponent<Servomotor>("GarraEsquerda").Locked = false;
    Bot.GetComponent<Servomotor>("GarraDireita").Apply(500, Velocidade);
    Bot.GetComponent<Servomotor>("GarraEsquerda").Apply(500, -Velocidade);
    await Time.Delay(200);
    // while(Bot.GetComponent<Servomotor>("GarraEsquerda").Angle > EsquerdaAberta && Bot.GetComponent<Servomotor>("GarraDireita").Angle < DireitaAberta) {
    //     Bot.GetComponent<Servomotor>("GarraDireita").Apply(500, -Velocidade);
    //     Bot.GetComponent<Servomotor>("GarraEsquerda").Apply(500, Velocidade);
    //     await Time.Delay(16);    
    // }
    // while(Bot.GetComponent<Servomotor>("GarraEsquerda").Angle > EsquerdaAberta) {
    //     Bot.GetComponent<Servomotor>("GarraEsquerda").Apply(500, Velocidade);
    //     await Time.Delay(16);    
    // }
    // while(Bot.GetComponent<Servomotor>("GarraDireita").Angle < DireitaAberta) {
    //     Bot.GetComponent<Servomotor>("GarraDireita").Apply(500, -Velocidade);
    //     await Time.Delay(16);    
    // }
    Bot.GetComponent<Servomotor>("GarraDireita").Locked = true;
    Bot.GetComponent<Servomotor>("GarraEsquerda").Locked = true;
}

async Task BaixarGarra() {
    Bot.GetComponent<Servomotor>("Garra2").Locked = false;
    int TempoInicio = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    while(Bot.GetComponent<Servomotor>("Garra2").Angle > 175 || Bot.GetComponent<Servomotor>("Garra2").Angle < 10) {
        Bot.GetComponent<Servomotor>("Garra2").Apply(500, -2000);
        await Time.Delay(16);
        if((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-TempoInicio > 150) {
            IO.Print(Bot.GetComponent<Servomotor>("Garra2").Angle.ToString());
            Bot.GetComponent<Servomotor>("Garra2").Locked = true;
            Bot.GetComponent<Servomotor>("Garra2").Locked = false;
            await Time.Delay(100);
            TempoInicio = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
    while(Bot.GetComponent<Servomotor>("Garra2").Angle > 50 || Bot.GetComponent<Servomotor>("Garra2").Angle < 10) {
        // IO.Print(Bot.GetComponent<Servomotor>("Garra2").Angle.ToString());
        Bot.GetComponent<Servomotor>("Garra2").Apply(500, -2000);
        await Time.Delay(16);
    }
    while(Bot.GetComponent<Servomotor>("Garra2").Angle > 35) {
        IO.Print(Bot.GetComponent<Servomotor>("Garra2").Angle.ToString());
        Bot.GetComponent<Servomotor>("Garra2").Apply(500, -200);
        await Time.Delay(16);
    }
    Bot.GetComponent<Servomotor>("Garra2").Locked = true;
    await Time.Delay(100);
    Bot.GetComponent<Servomotor>("Garra2").Locked = false;
    await Time.Delay(500);
    Bot.GetComponent<Servomotor>("Garra2").Locked = true;
}

async Task LevantarGarra() {
    int TempoInicio = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    int Forca = 340;
    Bot.GetComponent<Servomotor>("Garra2").Locked = false;
    int AnguloInicial = (int)Bot.GetComponent<Servomotor>("Garra2").Angle;
    while(Bot.GetComponent<Servomotor>("Garra2").Angle < AnguloInicial+30) {
        IO.Print((Math.Abs(Bot.GetComponent<Servomotor>("Garra2").Angle-AnguloInicial)).ToString());
        Bot.GetComponent<Servomotor>("Garra2").Apply(500, Forca);
        await Time.Delay(16);   
        // if((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-TempoInicio > 1000 && Math.Abs(Bot.GetComponent<Servomotor>("Garra2").Angle-AnguloInicial) < 14) {
        //     Forca += 10;
        //     await Time.Delay(100);
        //     TempoInicio = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
        // }   
    }
    Bot.GetComponent<Servomotor>("Garra2").Locked = true;
    
    // Bot.GetComponent<Servomotor>("Garra2").Locked = false;
    // Bot.GetComponent<Servomotor>("Garra2").Apply(500, 150);
    // await Time.Delay(666);
    // Bot.GetComponent<Servomotor>("Garra2").Locked = true;
}
async Task Entregar() {
    Bot.GetComponent<Servomotor>("Garra2").Locked = false;
    Bot.GetComponent<Servomotor>("Garra2").Apply(500, -600);
    await Time.Delay(1500);
    await AbrirGarra();
    Bot.GetComponent<Servomotor>("Garra2").Locked = true;
    await LevantarGarra();

}

async Task ErguerGarra() {
    Bot.GetComponent<Servomotor>("Garra1").Locked = false;
    Bot.GetComponent<Servomotor>("Garra1").Apply(500, 115);
    await Time.Delay(800);
    Bot.GetComponent<Servomotor>("Garra1").Locked = true;
}

async Task Girar(int Angulo, int Precisao) {
    await GirarDireita(200*(Angulo/Math.Abs(Angulo)));
	await Time.Delay(100);
	int VarObjetivo = await Objetivo(Angulo, Precisao);
	if(Bot.Compass > VarObjetivo && Angulo >= 0) {
		while(Bot.Compass > VarObjetivo) {
			await Time.Delay(10);
		}
		await Time.Delay(100);
	} else if(Bot.Compass < VarObjetivo && Angulo < 0) {
		while(Bot.Compass < VarObjetivo) {
			await Time.Delay(10);
		}
		await Time.Delay(100);
	}
	while((Bot.Compass > VarObjetivo && Angulo < 0) || (Bot.Compass < VarObjetivo && Angulo > 0)) {
		await Time.Delay(10);
        if(VarObjetivo > 350 && Bot.Compass < 15 && Angulo > 0) {
            break;
        } else if(VarObjetivo < 10 && Bot.Compass > 350 && Angulo < 0) {
            break;
        }
	}
	await Parar();
    await Time.Delay(50);
    await Destravar();
}

async Task GiroVerde(string SensorVerde, string OutroSensor, int ObjetivoTempo, int Forca) {
    bool DoisVerdes = false;
    bool Preto = false;
    int TempoInicial = 0;
	while(TempoInicial < ObjetivoTempo) {
        await Frente(150);
        await Time.Delay(50);
        TempoInicial += 50;
        if(Bot.GetComponent<ColorSensor>(OutroSensor).Analog.Green > Bot.GetComponent<ColorSensor>(OutroSensor).Analog.Blue + 20 && !Preto) {
            DoisVerdes = true;
        } else if(Bot.GetComponent<ColorSensor>(OutroSensor).Analog.Brightness < 55) {
            Preto = true;
        }
    }
    if(!DoisVerdes) {
        await GirarEsquerda(Forca);
        await Time.Delay(350);
        while(Bot.GetComponent<ColorSensor>(SensorVerde).Analog.Brightness > 55) {
            await GirarEsquerda(Forca);
            await Time.Delay(50);
        }
        await Time.Delay(70);
    } else {
        Forca = 250;
        SensorVerde = "CorEsquerda";
        await Girar(-135, 1);
        while(Bot.GetComponent<ColorSensor>(SensorVerde).Analog.Brightness > 55) {
            await GirarEsquerda(Forca);
            await Time.Delay(50);
        }
        await Time.Delay(70);
    }
    TempoInicial = 0;
    while(TempoInicial < 300) {
        if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness < 55 && Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green < Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue + 20) {
            await GirarDireita(250);
        } else if(Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness < 55 && Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Green < Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Blue + 20) {
            await GirarEsquerda(250);
        } else {
            await Frente(150);
        }
        await Time.Delay(50);
        TempoInicial += 50;
    }
    ValoresLuz = new[]{Luz(1), Luz(2)};
}

async Task Verde() {
    int TempoAndrade = 750;
    if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Red + 30) {
        await GiroVerde("CorDireita", "CorEsquerda", TempoAndrade, -250);
    } else if(Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Green > Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Red + 30) {
        await GiroVerde("CorEsquerda", "CorDireita", TempoAndrade, 250);
    }
}

async Task AndarReto(int Velocidade) {
    Bot.GetComponent<Servomotor>("MotorDireitaFrente").Apply(50, Velocidade);
    Bot.GetComponent<Servomotor>("MotorEsquerdaFrente").Apply(50, Velocidade);
    Bot.GetComponent<Servomotor>("MotorDireitaTras").Apply(50, Velocidade);
    Bot.GetComponent<Servomotor>("MotorEsquerdaTras").Apply(50, Velocidade);
    await Alinhar();
	// if(await Diferenca(0) < -0.2) {
	// 	if(ForcaFrente >= 0) {
	// 		await GirarMotores((float)(-2.63f*Math.Abs(await Diferenca(0))+293), 300);
	// 	} else {
	// 		await GirarMotores(-300, (float)(20*Math.Abs(await Diferenca(0))));
	// 	}
	// } else if(await Diferenca(0) > 0.2) {
	// 	if(ForcaFrente >= 0) {
	// 		await GirarMotores(300, (float)(-2.63f*Math.Abs(await Diferenca(0))+293));
	// 	} else {
	// 		await GirarMotores((float)(20*Math.Abs(await Diferenca(0))), -300);
	// 	}
	// } else {
	// 	await GirarMotores(ForcaFrente, ForcaFrente);
	// }
}

async Task Redzone() {
    await Frente(100);
    await Time.Delay(500);
    await Parar();
    double UltraFrente = Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog;
    await BaixarGarra();
    await Time.Delay(1000);
    await Destravar();
    await Alinhar();
    double Direita = 0;
    int TrianguloDireita = 0;
    int RedzoneDeitada = 0;
    int TempoInicio = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    while((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-TempoInicio < 4000) {
        Direita = Bot.GetComponent<UltrasonicSensor>("UltraDireita").Analog;
        await AndarReto(100);
        await Time.Delay(16);
        IO.Print((Direita-Bot.GetComponent<UltrasonicSensor>("UltraDireita").Analog).ToString());
        if(Direita-Bot.GetComponent<UltrasonicSensor>("UltraDireita").Analog > -0.1 && Bot.GetComponent<UltrasonicSensor>("UltraDireita").Analog-Direita > -0.5) {
            TrianguloDireita++;
        }
        if(Bot.GetComponent<UltrasonicSensor>("UltraDireita").Analog==-1) {
            RedzoneDeitada++;
        }
        IO.Print(RedzoneDeitada.ToString());
    }
    await Alinhar();
    if(RedzoneDeitada < 8 && UltraFrente == -1) { //de pé
        IO.Print("Redzone de pe");
        IO.Print(TrianguloDireita.ToString());
        if(TrianguloDireita > 8 && UltraFrente == -1) { //de pé direita
            while(true) {
                await Frente(100);
                await Time.Delay(16);
                IO.Print((Bot.GetComponent<UltrasonicSensor>("UltraDireita").Analog).ToString());
                if(Bot.GetComponent<UltrasonicSensor>("UltraDireita").Analog < 16) {
                    await Frente(200);
                    await Time.Delay(575);
                    await Girar(90, 0);
                    await Alinhar();
                    await Parar();
                    await Time.Delay(300);
                    await Destravar();
                    int Tempo_Inicial = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    while((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-Tempo_Inicial < 8800) {
                        await Frente(200);
                        await Time.Delay(16);
                    }
                    await Parar();
                    await FecharGarra();
                    await Time.Delay(100);
                    await LevantarGarra();
                    await Destravar();
                    Tempo_Inicial = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    while((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-Tempo_Inicial < 700) {
                        await Frente(200);
                        await Time.Delay(16);
                    }
                    await Parar();
                    int DistanciaTriangulo = (int)Bot.GetComponent<UltrasonicSensor>("UltraDireita").Analog;
                    await Destravar();
                    await Frente(-100);
                    await Time.Delay(1000);
                    await Girar(90, 0);
                    //await Alinhar();
                    // Bot.GetComponent<Servomotor>("Garra2").Locked = false;
                    // Bot.GetComponent<Servomotor>("Garra2").Apply(500, -100);
                    // await Time.Delay(200);
                    // Bot.GetComponent<Servomotor>("Garra2").Locked = true; 
                    await Time.Delay(80);
                    await Parar();
                    //await ErguerGarra();
                    await Time.Delay(80);
                    await Destravar();
                    await Time.Delay(80);
                    Tempo_Inicial = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    while((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-Tempo_Inicial < 400*DistanciaTriangulo) {
                        await Frente(200);
                        await Time.Delay(16);
                    }
                    await Girar(-40,1);
                    await Parar();
                    await Time.Delay(40);
                    await Destravar();
                    await Frente(100);
                    await Time.Delay(4000);
                    await Parar();
                    await Entregar();
                }
            }
        }
    } else if (RedzoneDeitada > 8 && UltraFrente > 0) { //deitada
        IO.Print("Redzone deitada");
    } else if (RedzoneDeitada > 20 && UltraFrente == -1) {
        IO.Print("estou perdido");
    }



    //     while(true) {
    //         await AndarReto(100);
    //         await Time.Delay(16);
    //         if(Bot.GetComponent<UltrasonicSensor>("UltraDireita").Analog < 16) {
    //             await Frente(100);
    //             await Time.Delay(2150);
    //             await Girar(90, 0);
    //             await Parar();
    //             await Time.Delay(300);
    //             await Destravar();
    //             int Tempo_Inicial = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    //             while((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-Tempo_Inicial < 10000) {
    //                 await AndarReto(300);
    //                 await Time.Delay(16);
    //             }
    //             await Parar();
    //             await Time.Delay(200);
    //             await Destravar();
    //             Tempo_Inicial = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    //             Bot.GetComponent<Servomotor>("Garra2").Locked = false;
    //             while((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-Tempo_Inicial < 900) {
    //                 Bot.GetComponent<Servomotor>("Garra2").Apply(500, -85);
    //                 await Tras(50);
    //                 await Time.Delay(16);
    //             }
    //             Bot.GetComponent<Servomotor>("Garra2").Locked = true;
    //             await Parar();
    //             await Time.Delay(3500);
    //             Tempo_Inicial = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    //             Bot.GetComponent<Servomotor>("Garra2").Locked = false;
    //             Bot.GetComponent<Servomotor>("Garra1").Locked = false;
    //             await Destravar();
    //             while((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-Tempo_Inicial < 2000) {
    //                 Bot.GetComponent<Servomotor>("Garra2").Apply(500, 100);
    //                 Bot.GetComponent<Servomotor>("Garra1").Apply(500, 120);
    //                 await Frente(250);
    //                 await Time.Delay(16);
    //             }
    //             Tempo_Inicial = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    //             while((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-Tempo_Inicial < 800) {
    //                 Bot.GetComponent<Servomotor>("Garra2").Apply(500, -85);
    //                 Bot.GetComponent<Servomotor>("Garra1").Apply(500, -85);
    //                 await Tras(50);
    //                 await Time.Delay(16);
    //             }
    //             await Parar();
    //             Bot.GetComponent<Servomotor>("Garra2").Locked = true;
    //             Bot.GetComponent<Servomotor>("Garra1").Locked = true;
    //             await Destravar();
    //             await Girar(90, 0);
    //             await Frente(150);
    //             await Time.Delay(10000);
    //             Bot.GetComponent<Servomotor>("Garra2").Locked = false;
    //             Bot.GetComponent<Servomotor>("Garra2").Apply(500, 85);
    //             await Time.Delay(20000);
    //         }
    //     }
    // }
}

async Task DoisPretos() {
    await Frente(100);
    await Time.Delay(50);
    if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Red + 30 || Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Green > Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Red + 30) {
        await Verde();
    } else if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Red +7 && Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue  > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green  && Bot.GetComponent<ColorSensor>("CorDireita").Analog.Red  < 90) {
        await Redzone();
    } else {
        int ValorGiro;
        if(Luz(1) > Luz(2)) {
            ValorGiro = -200;
        } else {
            ValorGiro = 200;
        }
        int TempoInicial = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
        bool Verde = false;
        int[] ValoresLuz = {0, 0};
		int[] ValoresTempo = {0, 0, 0};
        while((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-TempoInicial < 450) {
            await Frente(150);
            await Time.Delay(50);
            if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue + 20 || Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Green > Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Blue + 20) {
                Verde = true;
            }
            if(Luz(1) > 40 && ValoresLuz[0] == 0) {
                ValoresLuz[0] = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-TempoInicial;
            }
            if(Luz(2) > 40 && ValoresLuz[1] == 0) {
                ValoresLuz[1] = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-TempoInicial;
            }
        }
        if(ValoresLuz[0] > 150 || ValoresLuz[1] > 150 || ValoresLuz[0] == 0 || ValoresLuz[1] == 0) {
			ValoresTempo = new int[] {250, 1800, 3300};
		} else {
			ValoresTempo = new int[] {0, 1550, 3050};
		}
        bool ContinuarDoisPretos = true;
        if(ValoresTempo[0] != 0) {
            TempoInicial = 0;
            while(TempoInicial < ValoresTempo[0]) {
                if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness < 55 || Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness < 55) {
                    ContinuarDoisPretos = false;
                    break;
                }
                await GirarEsquerda(ValorGiro);
                await Time.Delay(50);
                TempoInicial += 50;
            }
        }
        TempoInicial = 0;
        if(ContinuarDoisPretos) {
            while(TempoInicial < ValoresTempo[1]) {
                if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness < 55 || Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness < 55) {
                    ContinuarDoisPretos = false;
                    break;
                }
                await GirarDireita(ValorGiro);
                await Time.Delay(50);
                TempoInicial += 50;
            }
            TempoInicial = 0;
            if(ContinuarDoisPretos) {
                while(TempoInicial < ValoresTempo[2]) {
                    if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness < 55 || Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness < 55) {
                        ContinuarDoisPretos = false;
                        break;
                    }
                    await GirarEsquerda(ValorGiro);
                    await Time.Delay(50);
                    TempoInicial += 50;
                }
            }
        }
        await Frente(0);
        TempoInicial = 0;
        Verde = true;
        if(Verde || Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue + 30 || Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Green > Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Blue + 30) {
            while(TempoInicial < 400) {
                if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness < 55 && Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness < 55) {
                    return;
                } else if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness < 55 && Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green < Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue + 20) {
                    await GirarDireita(300);
                } else if(Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness < 55 && Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Green < Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Blue + 20) {
                    await GirarEsquerda(300);
                } else {
                    await Frente(100);
                }
                await Time.Delay(50);
                TempoInicial += 50;
            }
        }
    }
    ValoresLuz = new[]{Luz(1), Luz(2)};
}

async Task Tendencioso(float ForcaDireita, float ForcaEsquerda) {
	await GirarMotores(ForcaDireita, ForcaEsquerda);
	if(VarTempoEsq > VarTempoDir && await Diferenca(0) < 0 && await Diferenca(0) > -32 && (int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-VarTempoDir > 350 && (int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-VarTempoEsq > 350) {
		VarTempoDir = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
		VarTempoTendencioso = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
		ValorTendencioso = -0.4f*Math.Abs(await Diferenca(0))-89;
	} else if(VarTempoEsq < VarTempoDir && await Diferenca(0) > 0 && await Diferenca(0) < 32 && (int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-VarTempoDir > 350 && (int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-VarTempoEsq > 350) {
		VarTempoEsq = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
		VarTempoTendencioso = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
		ValorTendencioso = -0.4f*Math.Abs(await Diferenca(0))-89;
	} else if((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-VarTempoTendencioso > 1000) {
		ValorTendencioso = -2.63f*Math.Abs(await Diferenca(0))+143;
	}
}

async Task Preto() {
    if((Luz(1) < 130 || Luz(1)-ValoresLuz[0] < -25) && Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green < Bot.GetComponent<ColorSensor>("CorDireita").Analog.Red + 30) {
        if(Luz(2) < 130 || Luz(1)-ValoresLuz[0] < -85) {
            await DoisPretos();
        } else {
            if(Math.Abs(Luz(1)-Luz(2)) < 50 || (Bot.Inclination < 355 && Bot.Inclination > 10)) {
                await GirarMotores((-2.63f*Math.Abs(Luz(1)-Luz(2))+143), 150);
            } else {
                await GirarDireita(300);
            }
            ValorTendencioso = -2.63f*Math.Abs(await Diferenca(0))+143;
			VarTempoDir = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    } else if((Luz(2) < 130 || Luz(2)-ValoresLuz[1] < -25) && Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Green < Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Red + 30) {
        if(Luz(1) < 130 || Luz(2)-ValoresLuz[1] < -85) {
            await DoisPretos();
        } else {
            if(Math.Abs(Luz(1)-Luz(2)) < 50 || (Bot.Inclination < 355 && Bot.Inclination > 10)) {
                await GirarMotores(150, (-2.63f*Math.Abs(Luz(1)-Luz(2))+143));
            } else {
                await GirarEsquerda(300);
            }
            ValorTendencioso = -2.63f*Math.Abs(await Diferenca(0))+143;
			VarTempoEsq = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
       }
    } else {
        // if(VarTempoEsq > VarTempoDir && Math.Abs(await Diferenca(0)) > 4) {
		// 	//await Tendencioso(150, ValorTendencioso);
        //     await Alinhar();
		// } else if(VarTempoDir > VarTempoEsq && Math.Abs(await Diferenca(0)) > 4) {
		// 	//await Tendencioso(ValorTendencioso, 150);
        //     await Alinhar();
		// } else if(Bot.Inclination < 355 && Bot.Inclination > 10) {
			await Frente(200);
		// } else {
        //     await Frente(150);
        //}
    }
    ValoresLuz = new[]{Luz(1), Luz(2)};
}

async Task Desviar(int Tempo) {
	VarTempo = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
	while(DateTimeOffset.Now.ToUnixTimeMilliseconds()-VarTempo < Tempo) {
		if(Luz(2) < 55) {
			DesvioVerdadeiro = false;
			break;
		} else {
			await Frente(150);
		}
        await Time.Delay(50);
	}
}

async Task Desvio() {
    if(Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog < 1.5 && Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog > 0 && (Bot.Inclination > 355 || Bot.Inclination < 10)) {
        await Time.Delay(50);
        if(Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog < 1.5 && Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog > 0 && (Bot.Inclination > 355 || Bot.Inclination < 10)) {
            while(Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog < 2.5) {
                await Frente(-50);
                await Time.Delay(50);
            }
            await Girar(60, 1);
            await Frente(150);
            await Time.Delay(2800);
            await Girar(-90, 0);
            await Frente(250);
            await Time.Delay(2300);
            await Girar(-60, 0);
            while(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness > 55) {
                await Frente(150);
                await Time.Delay(50);
            }
            await Frente(150);
            await Time.Delay(850);
            await Girar(90, 0);
            int TempoInicial = 0;
            while(TempoInicial < 300) {
                if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness < 55 && Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green < Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue + 20) {
                    await GirarDireita(250);
                } else if(Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness < 55 && Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Green < Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Blue + 20) {
                    await GirarEsquerda(250);
                } else {
                    await Frente(150);
                }
                await Time.Delay(50);
                TempoInicial += 50;
            }
            ValoresLuz = new[]{Luz(1), Luz(2)};
        }
    }
}

async Task Main() {
    IO.Print((Bot.GetComponent<Servomotor>("GarraEsquerda").Angle).ToString());
    await Destravar();
    while(true) {
        await Preto();
        await Verde();
        await Desvio();
        await Time.Delay(16);
    }
}