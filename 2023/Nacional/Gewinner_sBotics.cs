int[][] AngulosRetos = {new []{0, 90, 180, 270, 360}, new []{0, 45, 90, 135, 180, 225, 270, 315, 360}};

int Angulo(int Precisao) {
	double Direcao = Bot.Compass;
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

int Objetivo(int AnguloDesejado, int Precisao) {
	int AnguloAtual = Angulo(Precisao);
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

double Clamp(double Valor, int Min, int Max) {
    if(Valor < Min) {
        Valor = Min;
    } else if(Valor > Max) {
        Valor = Max;
    }
    return Valor;
}

void Destravar() {
    Bot.GetComponent<Servomotor>("frontLeftMotor").Locked = false;
    Bot.GetComponent<Servomotor>("leftMotor").Locked = false;
    Bot.GetComponent<Servomotor>("frontRightMotor").Locked = false;
    Bot.GetComponent<Servomotor>("rightMotor").Locked = false;
}

void Parar() {
    Bot.GetComponent<Servomotor>("frontLeftMotor").Locked = true;
    Bot.GetComponent<Servomotor>("leftMotor").Locked = true;
    Bot.GetComponent<Servomotor>("frontRightMotor").Locked = true;
    Bot.GetComponent<Servomotor>("rightMotor").Locked = true;
}

void MoverMotores(double ValorDir, double ValorEsq) {
    if((Bot.Inclination > 330 &&  Bot.Inclination < 350) || (Bot.Inclination > 5 &&  Bot.Inclination < 30)) {
        ValorDir *= 0.6;
        ValorEsq *= 0.6;
    }
    IO.Print(ValorDir.ToString()+" e "+ValorEsq.ToString());
    Bot.GetComponent<Servomotor>("frontLeftMotor").Apply(50, ValorEsq);
    Bot.GetComponent<Servomotor>("leftMotor").Apply(50, ValorEsq);
    Bot.GetComponent<Servomotor>("frontRightMotor").Apply(50, ValorDir);
    Bot.GetComponent<Servomotor>("rightMotor").Apply(50, ValorDir);
}

async Task Girar(int Angulo, int Precisao) {
	MoverMotores(-350*(Angulo/Math.Abs(Angulo)), 350*(Angulo/Math.Abs(Angulo)));
	await Time.Delay(100);
	int VarObjetivo = Objetivo(Angulo, Precisao);
	if(Bot.Compass > VarObjetivo && Angulo >= 0) {
		while(Bot.Compass > VarObjetivo) {
			await Time.Delay(25);
		}
		await Time.Delay(100);
	} else if(Bot.Compass < VarObjetivo && Angulo < 0) {
		while(Bot.Compass < VarObjetivo) {
			await Time.Delay(25);
		}
		await Time.Delay(100);
	}
	while((Bot.Compass > VarObjetivo && Angulo < 0) || (Bot.Compass < VarObjetivo && Angulo > 0)) {
		await Time.Delay(25);
	}
	MoverMotores(0, 0);
    await Time.Delay(100);
}

async Task TendenciosoComTimer() {//double Valor) {
    double Tempo_Inicial = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    while(DateTimeOffset.Now.ToUnixTimeMilliseconds() - Tempo_Inicial < 400) {
        double Diferenca = Bot.GetComponent<ColorSensor>("S1").Analog.Green - Bot.GetComponent<ColorSensor>("S2").Analog.Green;
        await Tendencioso(Diferenca);
        await Time.Delay(25);
    }
        // if(Valor > 10) {
    //     IO.Print("Tendencioso");
    //     MoverMotores((-2*Valor)+225, 225);
    // } else if(Valor < -10) {
    //     IO.Print("Tendencioso");
    //     MoverMotores(225, (2*Valor)+225);
    // } else {
    //     IO.Print("Frente");
    //     MoverMotores(225, 225);
    // }
}

async Task Tendencioso(double Valor) {
    // IO.Print(Valor.ToString());
     if(Valor > 10) {
        // IO.Print("Tendencioso");
        if((Bot.Inclination > 330 &&  Bot.Inclination < 350) || (Bot.Inclination > 5 &&  Bot.Inclination < 30)) {
            Valor /= 2;
        }
        MoverMotores((-2*Valor)+225, 225);
    } else if(Valor < -10) {
        if((Bot.Inclination > 330 &&  Bot.Inclination < 350) || (Bot.Inclination > 5 &&  Bot.Inclination < 30)) {
            Valor /= 2;
        }
        // IO.Print("Tendencioso");
        MoverMotores(225, (2*Valor)+225);
    } else {
        // IO.Print("Frente");
        MoverMotores(225, 225);
    }
}

async Task Verde() {
    if(Bot.GetComponent<ColorSensor>("S1").Analog.Green-Bot.GetComponent<ColorSensor>("S1").Analog.Red > 10 || (Bot.GetComponent<ColorSensor>("S1").Analog.Green > 70 && Bot.GetComponent<ColorSensor>("S1").Analog.Red < 40)) {
        IO.Print("Verde!");
        double Tempo_Inicial = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        bool Dois_Verdes = false;
        bool Passou_Linha_Preta = false;
        while(DateTimeOffset.Now.ToUnixTimeMilliseconds() - Tempo_Inicial < 400) {
            MoverMotores(250, 250);
            if(!Passou_Linha_Preta && Bot.GetComponent<ColorSensor>("S2").Analog.Green > Bot.GetComponent<ColorSensor>("S2").Analog.Red + 20) {
                Dois_Verdes = true;
            } else if(Bot.GetComponent<ColorSensor>("S1").Analog.Green < 30) {
                Passou_Linha_Preta = true;
            }
            await Time.Delay(25);
        }
        MoverMotores(350, -350);
        await Time.Delay(200);
        if(Dois_Verdes) {
            IO.Print("Dois Verdes!");
            MoverMotores(350, -350);
            await Time.Delay(1000);
        }
        while(Bot.GetComponent<ColorSensor>("S1").Analog.Green > 30) {
            MoverMotores(350, -350);
            await Time.Delay(25);
        }
        // Tempo_Inicial = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        // while(DateTimeOffset.Now.ToUnixTimeMilliseconds() - Tempo_Inicial < 400) {
        //     double Diferenca = Bot.GetComponent<ColorSensor>("S1").Analog.Green - Bot.GetComponent<ColorSensor>("S2").Analog.Green;
        await TendenciosoComTimer();//Diferenca);
        //     await Time.Delay(25);
        // }
    } else if(Bot.GetComponent<ColorSensor>("S2").Analog.Green-Bot.GetComponent<ColorSensor>("S2").Analog.Red > 10 || (Bot.GetComponent<ColorSensor>("S2").Analog.Green > 70 && Bot.GetComponent<ColorSensor>("S2").Analog.Red < 40)) {
        IO.Print("Verde!");
        double Tempo_Inicial = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        bool Dois_Verdes = false;
        bool Passou_Linha_Preta = false;
        while(DateTimeOffset.Now.ToUnixTimeMilliseconds() - Tempo_Inicial < 400) {
            MoverMotores(250, 250);
            if(!Passou_Linha_Preta && Bot.GetComponent<ColorSensor>("S1").Analog.Green > Bot.GetComponent<ColorSensor>("S1").Analog.Red + 20) {
                Dois_Verdes = true;
            } else if(Bot.GetComponent<ColorSensor>("S1").Analog.Green < 30) {
                Passou_Linha_Preta = true;
            }
            await Time.Delay(25);
        }
        MoverMotores(-350, 350);
        await Time.Delay(200);
        if(Dois_Verdes) {
            IO.Print("Dois Verdes!");
            MoverMotores(-350, 350);
            await Time.Delay(1000);
        }
        while(Bot.GetComponent<ColorSensor>("S2").Analog.Green > 30) {
            MoverMotores(-350, 350);
            await Time.Delay(25);
        }
        await TendenciosoComTimer();
    //     Tempo_Inicial = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    //     while(DateTimeOffset.Now.ToUnixTimeMilliseconds() - Tempo_Inicial < 400) {
    //         double Diferenca = Bot.GetComponent<ColorSensor>("S1").Analog.Green - Bot.GetComponent<ColorSensor>("S2").Analog.Green;
    //         await Tendencioso(Diferenca);
    //         await Time.Delay(25);
    //     }
    }
}

async Task Preto() {
    double Diferenca = Bot.GetComponent<ColorSensor>("S1").Analog.Green - Bot.GetComponent<ColorSensor>("S2").Analog.Green;
    if(Bot.GetComponent<ColorSensor>("S3").Analog.Green < 100) {
        double Tempo_Inicial = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        bool Interseccao = false;
        while(DateTimeOffset.Now.ToUnixTimeMilliseconds() - Tempo_Inicial < 200) {
            MoverMotores(250, 250);
            await Time.Delay(25);
            if(Bot.GetComponent<ColorSensor>("S0").Analog.Green < 100) {
                Interseccao = true;
            }
        }
        if(!Interseccao) {
            IO.Print("Curva Direita");
            Parar();
            await Time.Delay(25);
            Destravar();
            while(Bot.GetComponent<ColorSensor>("S2").Analog.Green > 100 && Bot.GetComponent<ColorSensor>("S1").Analog.Green > 100) {
                MoverMotores(-350, 350);
                await Time.Delay(25);
            }
        }
        await TendenciosoComTimer();
        // Tempo_Inicial = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        // while(DateTimeOffset.Now.ToUnixTimeMilliseconds() - Tempo_Inicial < 400) {
        //     Diferenca = Bot.GetComponent<ColorSensor>("S1").Analog.Green - Bot.GetComponent<ColorSensor>("S2").Analog.Green;
        //     await Tendencioso(Diferenca);
        //     await Time.Delay(25);
        // }
    } else if(Bot.GetComponent<ColorSensor>("S0").Analog.Green < 100) {
        double Tempo_Inicial = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        bool Interseccao = false;
        while(DateTimeOffset.Now.ToUnixTimeMilliseconds() - Tempo_Inicial < 200) {
            MoverMotores(250, 250);
            await Time.Delay(25);
            if(Bot.GetComponent<ColorSensor>("S3").Analog.Green < 100) {
                Interseccao = true;
            }
        }
        if(!Interseccao) {
            IO.Print("Curva Esquerda");
            Parar();
            await Time.Delay(25);
            Destravar();
            while(Bot.GetComponent<ColorSensor>("S1").Analog.Green > 100 && Bot.GetComponent<ColorSensor>("S2").Analog.Green > 100) {
                MoverMotores(350, -350);
                await Time.Delay(25);
            }
        }
        await TendenciosoComTimer();
        // Tempo_Inicial = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        // while(DateTimeOffset.Now.ToUnixTimeMilliseconds() - Tempo_Inicial < 400) {
        //     Diferenca = Bot.GetComponent<ColorSensor>("S1").Analog.Green - Bot.GetComponent<ColorSensor>("S2").Analog.Green;
        //     await Tendencioso(Diferenca);
        //     await Time.Delay(25);
        // }
    } else if(Bot.GetComponent<ColorSensor>("S1").Analog.Green < 40) {
        MoverMotores(350, -350);
    } else if(Bot.GetComponent<ColorSensor>("S2").Analog.Green < 40) {
        MoverMotores(-350, 350);
    } else {
        await Tendencioso(Diferenca);
    }
}

async Task Desvio() {
    if(Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog > 0 && Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog < 1.5) {
        while(Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog < 2.5 && Bot.GetComponent<UltrasonicSensor>("UltraFrente").Analog > 0) {
            MoverMotores(-150, -150);
            await Time.Delay(25);
        }
        await Girar(45, 1);
        MoverMotores(300, 300);
        await Time.Delay(1250);
        await Girar(-45, 1);
        double Tempo_Inicial = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        bool Viu_Preto = false;
        while(DateTimeOffset.Now.ToUnixTimeMilliseconds() - Tempo_Inicial < 1500) {
            MoverMotores(300, 300);
            await Time.Delay(25);
            if(Bot.GetComponent<ColorSensor>("S1").Analog.Green < 30) {
                Viu_Preto = true;
                break;
            }
        }
        if(Viu_Preto) {
            MoverMotores(300, 300);
            await Time.Delay(300);
            await Girar(90, 1);
        } else {
            await Girar(-45, 1);
        }
    }
}

async Task Main() {
    Destravar();
    while(true) {
        await Preto();
        await Verde();
        await Desvio();
        await Time.Delay(16);
    }
}