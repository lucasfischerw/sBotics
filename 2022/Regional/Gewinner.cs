int[][] AngulosRetos = {new []{0, 90, 180, 270, 360}, new []{0, 45, 90, 135, 180, 225, 270, 315, 360}};
int VarTempo = 0;

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

async Task<int> Objetivo(int AnguloDesejado, int Precisao) {
	int AnguloAtual = await Angulo(Precisao);
	if(AnguloDesejado >= 0) {
		return ((AnguloAtual+AnguloDesejado)-7)%360;
	} else {
		if(((AnguloAtual+AnguloDesejado)+7)%360 >= 0) {
			return ((AnguloAtual+AnguloDesejado)+7)%360;
		} else {
			return 360+((AnguloAtual+AnguloDesejado)+7);
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

async Task Girar(int Angulo, int Precisao) {
    await GirarDireita(200*(Angulo/Math.Abs(Angulo)));
	await Time.Delay(100);
	int VarObjetivo = await Objetivo(Angulo, Precisao);
    IO.Print(VarObjetivo.ToString());
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
        IO.Print(Bot.Compass.ToString());
	}
	await Parar();
    await Time.Delay(50);
    await Destravar();
}

async Task DoisPretos() {
    if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Red +7 && Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue  > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green  && Bot.GetComponent<ColorSensor>("CorDireita").Analog.Red  < 90) {
        await Frente(100);
        await Time.Delay(5000);
    } else {
        int TempoInicial = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
        bool Verde = false;
        while(DateTimeOffset.Now.ToUnixTimeMilliseconds()-TempoInicial < 400) {
            await Frente(150);
            await Time.Delay(50);
            if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue + 20 || Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Green > Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Blue + 20) {
                Verde = true;
            }
        }
        await Frente(200);
        await Time.Delay(400);
        TempoInicial = 0;
        bool ContinuarDoisPretos = true;
        while(TempoInicial < 250) {
            if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness < 55 || Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness < 55) {
                ContinuarDoisPretos = false;
                break;
            }
            GirarEsquerda(200);
            await Time.Delay(50);
            TempoInicial += 50;
        }
        TempoInicial = 0;
        if(ContinuarDoisPretos) {
            while(TempoInicial < 1500) {
                if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness < 55 || Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness < 55) {
                    ContinuarDoisPretos = false;
                    break;
                }
                GirarDireita(200);
                await Time.Delay(50);
                TempoInicial += 50;
            }
            TempoInicial = 0;
            if(ContinuarDoisPretos) {
                while(TempoInicial < 3000) {
                    if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness < 55 || Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness < 55) {
                        ContinuarDoisPretos = false;
                        break;
                    }
                    GirarEsquerda(200);
                    await Time.Delay(50);
                    TempoInicial += 50;
                }
            }
        }
        await Parar();
        await Time.Delay(100);
        await Frente(0);
        await Destravar();
        TempoInicial = 0;
        if(Verde || Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue + 30 || Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Green > Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Blue + 30) {
            while(TempoInicial < 200) {
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
        }
    }
}

async Task Preto() {
    if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness < 100 && Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green < Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue + 20) {
        if(Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness < 100 || (Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness < 10 && Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness < 150)) {
            await DoisPretos();
        } else {
            if(Bot.Inclination < 355 && Bot.Inclination > 10) {
                await GirarDireita(100);
            } else {
                await GirarDireita(200);
            }
        }
    } else if(Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness < 100 && Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Green < Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Blue + 20) {
        if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness < 100 || (Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Brightness < 10 && Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness < 150)) {
            await DoisPretos();
        } else {
            if(Bot.Inclination < 355 && Bot.Inclination > 10) {
                await GirarEsquerda(100);
            } else {
                await GirarEsquerda(200);
            }
       }
    } else if(Bot.Speed < 1.6) {
        if(Bot.Speed < 0.4) {
            await Frente(300);
        } else {
            await Frente(100);
        }
    } else {
        await Frente(0);
    }
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
}

async Task Verde() {
    if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue + 30) {
        await GiroVerde("CorDireita", "CorEsquerda", 400, -250);
    } else if(Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Green > Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Blue + 30) {
        await GiroVerde("CorEsquerda", "CorDireita", 400, 250);
    }
}

async Task Desvio() {
    if(Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog < 2 && Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog > 0 && (Bot.Inclination > 355 || Bot.Inclination < 10)) {
        await Time.Delay(50);
        if(Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog < 2 && Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog > 0 && (Bot.Inclination > 355 || Bot.Inclination < 10)) {
            await Frente(-150);
            await Time.Delay(350);
            await Girar(90, 0);
            await Frente(150);
            await Time.Delay(2800);
            await Girar(-90, 0);
            await Frente(150);
            await Time.Delay(5000);
            await Girar(-90, 0);
            while(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness > 55) {
                await Frente(150);
                await Time.Delay(50);
            }
            await Frente(150);
            await Time.Delay(400);
            await Girar(90, 0);
            // while(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness > 55) {
            //     await GirarDireita(250);
            //     await Time.Delay(50);
            // }
        }
    }
}

async Task Main() {
    // await LevantaGarra();
    await Destravar();
    while(true) {
        await Verde();
        await Preto();
        await Desvio();
        IO.Print(Bot.Speed.ToString());
        await Time.Delay(50);
    }
}