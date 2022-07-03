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
		return ((AnguloAtual+AnguloDesejado)-2)%360;
	} else {
		if(((AnguloAtual+AnguloDesejado)+2)%360 >= 0) {
			return ((AnguloAtual+AnguloDesejado)+2)%360;
		} else {
			return 360+((AnguloAtual+AnguloDesejado)+2);
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
    await GirarDireita(250*(Angulo/Math.Abs(Angulo)));
	await Time.Delay(100);
	int VarObjetivo = await Objetivo(Angulo, Precisao);
	if(Bot.Compass > VarObjetivo && Angulo >= 0) {
		while(Bot.Compass > VarObjetivo) {
			await Time.Delay(50);
		}
		await Time.Delay(100);
	} else if(Bot.Compass < VarObjetivo && Angulo < 0) {
		while(Bot.Compass < VarObjetivo) {
			await Time.Delay(50);
		}
		await Time.Delay(100);
	}
	while((Bot.Compass > VarObjetivo && Angulo < 0) || (Bot.Compass < VarObjetivo && Angulo > 0)) {
		await Time.Delay(50);
	}
	await Parar();
    await Time.Delay(50);
    await Destravar();
}

async Task Girar90Graus(int Velocidade) {
    await Frente(100);
    await Time.Delay(1300);
    while(Bot.GetComponent<ColorSensor>("CorDireita1").Analog.Brightness > 55 && Bot.GetComponent<ColorSensor>("CorEsquerda1").Analog.Brightness > 55) {
        await GirarDireita(Velocidade);
        await Time.Delay(50);
    }
    await Parar();
    await Time.Delay(50);
    await Destravar();
}

async Task Preto() {
    if(Bot.GetComponent<ColorSensor>("CorDireita1").Analog.Brightness < 55 && Bot.GetComponent<ColorSensor>("CorDireita1").Analog.Green < Bot.GetComponent<ColorSensor>("CorDireita1").Analog.Blue + 20) {
        if(Bot.GetComponent<ColorSensor>("CorDireita2").Analog.Brightness < 55) {
            await Girar90Graus(250);
        } else {
            await GirarDireita(250);
        }
    } else if(Bot.GetComponent<ColorSensor>("CorEsquerda1").Analog.Brightness < 55 && Bot.GetComponent<ColorSensor>("CorEsquerda1").Analog.Green < Bot.GetComponent<ColorSensor>("CorEsquerda1").Analog.Blue + 20) {
        if(Bot.GetComponent<ColorSensor>("CorEsquerda2").Analog.Brightness < 55) {
            await Girar90Graus(-250);
        } else {
            await GirarEsquerda(250);
        }
    } else {
        await Frente(150);
    }
}

async Task GiroVerde(string SensorVerde, string OutroSensor, int ObjetivoTempo, int Forca) {
    bool DoisVerdes = false;
    bool Preto = false;
    VarTempo = (int)Time.Timestamp;
	while(Time.Timestamp-VarTempo < ObjetivoTempo) {
        await Frente(150);
        await Time.Delay(50);
        if(Bot.GetComponent<ColorSensor>(OutroSensor).Analog.Green > Bot.GetComponent<ColorSensor>(OutroSensor).Analog.Blue + 20 && !Preto) {
            DoisVerdes = true;
        } else if(Bot.GetComponent<ColorSensor>(OutroSensor).Analog.Brightness < 55) {
            Preto = true;
        } 
        if((int)Time.Timestamp-VarTempo > ObjetivoTempo) {
            break;
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
        SensorVerde = "CorEsquerda1";
        await Girar(-135, 1);
        while(Bot.GetComponent<ColorSensor>(SensorVerde).Analog.Brightness > 55) {
            await GirarEsquerda(Forca);
            await Time.Delay(50);
        }
        await Time.Delay(70);
    }
}

async Task Verde() {
    if(Bot.GetComponent<ColorSensor>("CorDireita1").Analog.Green > Bot.GetComponent<ColorSensor>("CorDireita1").Analog.Blue + 30) {
        await GiroVerde("CorDireita1", "CorEsquerda1", 2, -250);
    } else if(Bot.GetComponent<ColorSensor>("CorEsquerda1").Analog.Green > Bot.GetComponent<ColorSensor>("CorEsquerda1").Analog.Blue + 30) {
        await GiroVerde("CorEsquerda1", "CorDireita1", 2, 250);
    }
}

async Task Desvio() {
    IO.Print(Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog.ToString());
    if(Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog < 2 && Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog > 0) {
        await Girar(90, 0);
        await Frente(150);
        await Time.Delay(2500);
        await Girar(-90, 0);
        await Frente(150);
        await Time.Delay(6000);
        await Girar(-90, 0);
        while(Bot.GetComponent<ColorSensor>("CorDireita1").Analog.Brightness > 55) {
            await Frente(150);
            await Time.Delay(50);
        }
        await Frente(150);
        await Time.Delay(400);
        while(Bot.GetComponent<ColorSensor>("CorDireita1").Analog.Brightness > 55) {
            await GirarDireita(250);
            await Time.Delay(50);
        }
    }
}

async Task Main() {
    await Destravar();
    while(true) {
        await Verde();
        await Preto();
        await Desvio();
        await Time.Delay(50);
    }
}