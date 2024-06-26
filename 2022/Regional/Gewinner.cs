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
    return (float)(Bot.Compass-(await Angulo((int)Precisao)));
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

async Task GirarMotores(float ForcaDireita, float ForcaEsquerda) {
    Bot.GetComponent<Servomotor>("MotorDireitaFrente").Apply(50, ForcaDireita);
    Bot.GetComponent<Servomotor>("MotorEsquerdaFrente").Apply(50, ForcaEsquerda);
    Bot.GetComponent<Servomotor>("MotorDireitaTras").Apply(50, ForcaDireita);
    Bot.GetComponent<Servomotor>("MotorEsquerdaTras").Apply(50, ForcaEsquerda);
}

async Task Girar(int Angulo, int Precisao) {
    await GirarDireita(350*(Angulo/Math.Abs(Angulo)));
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

async Task LevantaGarra() {
	Bot.GetComponent<Servomotor>("Garra2").Locked = false;
	Bot.GetComponent<Servomotor>("Garra2").Apply(-500, -150);
	await Time.Delay(1000);
	Bot.GetComponent<Servomotor>("Garra2").Locked = true;
}

async Task GiroVerde(string SensorVerde, string OutroSensor, int ObjetivoTempo, int Forca) {
    bool DoisVerdes = false;
    bool Preto = false;
    int TempoInicial = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
	while((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()-TempoInicial < ObjetivoTempo) {
        await Frente(150);
        await Time.Delay(50);
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
    if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Red + 30) {
        await GiroVerde("CorDireita", "CorEsquerda", 700, -250);
    } else if(Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Green > Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Red + 30) {
        await GiroVerde("CorEsquerda", "CorDireita", 700, 250);
    }
}


async Task DoisPretos() {
    await Frente(100);
    await Time.Delay(50);
    if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Red + 30 || Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Green > Bot.GetComponent<ColorSensor>("CorEsquerda").Analog.Red + 30) {
        await Verde();
    } else if(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Red +7 && Bot.GetComponent<ColorSensor>("CorDireita").Analog.Blue  > Bot.GetComponent<ColorSensor>("CorDireita").Analog.Green  && Bot.GetComponent<ColorSensor>("CorDireita").Analog.Red  < 90) {
        await Frente(100);
        await Time.Delay(5000);
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
        if(VarTempoEsq > VarTempoDir && Math.Abs(await Diferenca(0)) > 1) {
			await Tendencioso(150, ValorTendencioso);
		} else if(VarTempoDir > VarTempoEsq && Math.Abs(await Diferenca(0)) > 1) {
			await Tendencioso(ValorTendencioso, 150);
		} else if(Bot.Inclination < 355 && Bot.Inclination > 10) {
			await Frente(300);
		} else {
            await Frente(150);
        }
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
            await Time.Delay(2600);
            await Girar(-90, 0);
            await Frente(150);
            await Time.Delay(2250);
            await Girar(-60, 0);
            while(Bot.GetComponent<ColorSensor>("CorDireita").Analog.Brightness > 55) {
                await Frente(150);
                await Time.Delay(50);
            }
            await Frente(150);
            await Time.Delay(250);
            await Girar(45, 1);
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
    // await LevantaGarra();
    await Time.Delay(300);
    await Destravar();
    while(true) {
        await Preto();
        await Verde();
        await Desvio();
        await Time.Delay(50);
    }
}

