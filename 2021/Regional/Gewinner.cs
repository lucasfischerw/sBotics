int varTempo = 0;
int varTempoDir = 0;
int varTempoEsq = 0;
int vezesGiroDir = 0;
int vezesGiroEsq = 0;
int ultimoLadoVerde = 0;
int ultimoLadoDoisPretos = 0;
int tempoUltimoVerde = 0;
int tempoUltimoDoisPretos = 0;
float velocidade = 130;
float anguloUltimoVerde = 0;
float anguloInicialUltimoVerde = 0;
float valorTendencioso = 0;
double diferenca = 0;
bool continuar = false;
bool desvio = false;
bool ladrilhoQuadrado = false;

float ultrassonico = 0;
float ultrassonicoFrente = 0;
float ultrassonicoBolinha = 0;
float probabilidadeSaida = 0;
float probabilidadeTriangulo = 0;
float posicaoTriangulo = 0;
float casoEspecial = 0;
float distanciaEntrega = 0;
int posicaoTrianguloOriginal = 0;
bool buscarBolinha = true;
bool redzoneCompleta = false;
bool bolinhaGrudada = false;

float Calibracao1() {
	if(bc.Lightness(0) > 99) {
		return 99;
	} else if(bc.Lightness(0)-5 > 55) {
		return bc.Lightness(0)-5;
	}
	return 55;
}

float Calibracao2() {
	if(bc.Lightness(1) > 99) {
		return 99;
	} else if(bc.Lightness(1)-5 > 55) {
		return bc.Lightness(1)-5;
	}
	return 55;
}

int Angulo() {
	if(bc.Compass() <= 45 || bc.Compass() >= 315) {
		return 0;
	} else if(bc.Compass() > 45 && bc.Compass() <= 135) {
		return 90;
	} else if(bc.Compass() > 135 && bc.Compass() <= 225) {
		return 180;
	} else {
		return 270;
	}
}

int Angulo2() {
	if(bc.Compass() <= 22.5 || bc.Compass() >= 337.5) {
		return 0;
	} else if(bc.Compass() > 22.5 && bc.Compass() <= 67.5) {
		return 45;
	} else if(bc.Compass() > 67.5 && bc.Compass() <= 112.5) {
		return 90;
	} else if(bc.Compass() > 112.5 && bc.Compass() <= 157.5) {
		return 135;
	} else if(bc.Compass() > 157.5 && bc.Compass() <= 202.5) {
		return 180;
	} else if(bc.Compass() > 202.5 && bc.Compass() <= 247.5) {
		return 225;
	} else if(bc.Compass() > 247.5 && bc.Compass() <= 292.5) {
		return 270;
	} else if(bc.Compass() <= 22.5 || bc.Compass() >= 337.5) {
		return 0;
	} else {
		return 315;
	}
}

double Diferenca() {
	if(bc.Compass() >= 337.5) {
		return 360 - bc.Compass();
	} else {
		return Math.Abs(bc.Compass() - Angulo2());
	}
}

float Objetivo(float objetivo) {
	float angulo;
	if(objetivo == 45 || objetivo == -45) {
		angulo = Angulo2();
	} else {
		angulo = Angulo();
	}
	if(objetivo <= 0) {
		objetivo = (angulo - Math.Abs(objetivo))+1;
		if(objetivo < 0) {
			objetivo = (360-Math.Abs(objetivo));
		}
	} else {
		objetivo = (angulo + objetivo)-1;
		if(objetivo >= 360) {
			objetivo = (Math.Abs(objetivo) - 360);
		}
	}
	return objetivo;
}

void Girar(float giro) {
	float objetivo = Objetivo(giro);
	if(giro > 0) {
		bc.MoveFrontal(-1000, 1000);
		bc.Wait(100);
		if(bc.Compass() > objetivo) {
			while(bc.Compass() > objetivo) {
				bc.MoveFrontal(-1000, 1000);
			}
		}
		while(bc.Compass() < objetivo) {
			bc.MoveFrontal(-1000, 1000);
		}
	} else {
		bc.MoveFrontal(1000, -1000);
		bc.Wait(100);
		if(bc.Compass() < objetivo) {
			while(bc.Compass() < objetivo) {
				bc.MoveFrontal(1000, -1000);
			}
		}
		while(bc.Compass() > objetivo) {
			bc.MoveFrontal(1000, -1000);
		}
	}
	bc.MoveFrontal(0, 0);
}

void DoisVerdes() {
	bc.MoveFrontalRotations(velocidade, 14);
	bc.MoveFrontalAngles(1000, 120);
	while(bc.ReturnColor(0) != "PRETO") {
		bc.MoveFrontal(-1000, 1000);
	}
	while(bc.ReturnColor(0) != "BRANCO") {
		bc.MoveFrontal(-1000, 1000);
	}
	varTempoDir = bc.Timer();
	varTempoEsq = varTempoDir;
}

void GiroVerde(int lado, int giro, int sensor) {
	anguloInicialUltimoVerde = Angulo2();
	ultimoLadoVerde = sensor+1;
	bool torto;
	if(Angulo2() == 0 || Angulo2() == 90 || Angulo2() == 180 || Angulo2() == 270) {
		torto = false;
		bc.MoveFrontalRotations(velocidade, 14);
		bc.MoveFrontalAngles(1000, lado);
	} else {
		torto = true;
		bc.MoveFrontalRotations(velocidade, 12);
	}
	if(bc.ReturnColor(sensor) == "PRETO") {
		while(bc.ReturnColor(sensor) == "PRETO") {
			bc.MoveFrontal(giro, -giro);
		}
	}
	while(bc.ReturnColor(sensor) != "PRETO") {
		bc.MoveFrontal(giro, -giro);
	}
	while(bc.ReturnColor(sensor) == "PRETO") {
		bc.MoveFrontal(giro, -giro);
	}
	bc.Wait(70);
	if(!torto) {
		varTempoDir = bc.Timer();
		varTempoEsq = varTempoDir;
	} else {
		if(ultimoLadoVerde == 1) {
			varTempoDir = bc.Timer();
		} else {
			varTempoEsq = bc.Timer();
		}
	}
	anguloUltimoVerde = Angulo();
	ladrilhoQuadrado = false;
	tempoUltimoVerde = bc.Timer();
}

void Gap(float objetivo) {
	float giro = Objetivo(objetivo);
	if(objetivo > 0) {
		if(bc.Compass() > giro) {
			while(bc.Compass() > giro) {
				if(bc.ReturnColor(0) == "PRETO" || bc.ReturnColor(1) == "PRETO") {
					continuar = false;
					ladrilhoQuadrado = true;
					break;
				} else {
					bc.MoveFrontal(-1000, 1000);
				}
			}
			bc.Wait(50);
		}
		while(bc.Compass() < giro) {
			if(bc.ReturnColor(0) == "PRETO" || bc.ReturnColor(1) == "PRETO") {
				continuar = false;
				ladrilhoQuadrado = true;
				break;
			} else {
				bc.MoveFrontal(-1000, 1000);
			}
		}
		if(continuar) {
			bc.MoveFrontal(0, 0);
			varTempo = bc.Timer();
			while(bc.Timer() - varTempo < 600) {
				if(objetivo == -17 || objetivo == 17) {
					bc.MoveFrontal(300, -20);
				} else {
					bc.MoveFrontal(-20, 300);
				}
			}
			bc.MoveFrontal(0, 0);
			continuar = false;
		}
	} else {
		if(bc.Compass() < giro) {
			while(bc.Compass() < giro) {
				if(bc.ReturnColor(0) == "PRETO" || bc.ReturnColor(1) == "PRETO") {
					continuar = false;
					ladrilhoQuadrado = true;
					break;
				} else {
					bc.MoveFrontal(1000, -1000);
				}
			}
			bc.Wait(50);
		}
		while(bc.Compass() > giro) {
			if(bc.ReturnColor(0) == "PRETO" || bc.ReturnColor(1) == "PRETO") {
				continuar = false;
				ladrilhoQuadrado = true;
				break;
			} else {
				bc.MoveFrontal(1000, -1000);
			}
		}
		if(continuar) {
			bc.MoveFrontal(0, 0);
			varTempo = bc.Timer();
			while(bc.Timer() - varTempo < 600) {
				if(objetivo == -17 || objetivo == 17) {
					bc.MoveFrontal(-20, 300);
				} else {
					bc.MoveFrontal(300, -20);
				}
			}
			bc.MoveFrontal(0, 0);
			continuar = false;
		}
	}
}

void GiroPreto(float lado, float objetivoTempo) {
	while(bc.Timer() - varTempo < objetivoTempo) {
		if(lado == 1) {
			bc.MoveFrontal(-1000, 1000);
		} else {
			bc.MoveFrontal(1000, -1000);
		}
		if(bc.ReturnColor(0) == "PRETO") {
			bc.MoveFrontal(0, 0);
			continuar = false;
			break;
		} else if(bc.ReturnColor(1) == "PRETO") {
			bc.MoveFrontal(0, 0);
			continuar = false;
			break;
		}
	}
}

void DoisPretos() {
	bc.MoveFrontal(0, 0);
	bc.Wait(25);
	if((bc.ReturnGreen(0) > 50 && bc.ReturnGreen(0) > bc.ReturnRed(0) + 30) || (bc.ReturnGreen(1) > 50 && bc.ReturnGreen(1) > bc.ReturnRed(1) + 30)) {
		bc.Wait(30);
		if((bc.ReturnGreen(0) > 50 && bc.ReturnGreen(0) > bc.ReturnRed(0) + 30) || (bc.ReturnGreen(1) > 50 && bc.ReturnGreen(1) > bc.ReturnRed(1) + 30)) {
			Verde();
			return;
		}
	} else if(bc.Inclination() < 341 && bc.Inclination() > 330 && (bc.ReturnColor(0) == "BRANCO" || bc.ReturnColor(1) == "BRANCO")) {
		bc.MoveFrontal(150, 150);
		bc.Wait(800);
		bc.MoveFrontal(0, 0);
	} else if(redzoneCompleta && (bc.ReturnRed(0) > bc.ReturnGreen(0) + 15 || bc.ReturnRed(1) > bc.ReturnGreen(1) + 15 || bc.ReturnColor(0) == "VERMELHO" || bc.ReturnColor(1) == "VERMELHO")) {
		return;
	} else {
		if(vezesGiroDir > vezesGiroEsq && vezesGiroEsq != 0 && varTempoEsq > varTempoDir) {
			vezesGiroEsq = vezesGiroDir+10;
		}
		float verde = bc.Timer()-tempoUltimoVerde;
		float preto = bc.Timer()-tempoUltimoDoisPretos;
		float angulo = Angulo();
		bc.MoveFrontal(150, 150);
		bc.Wait(350);
		bc.MoveFrontal(0, 0);
		continuar = true;
		varTempo = bc.Timer();
		if(vezesGiroEsq > vezesGiroDir) {
			GiroPreto(1, 300);
			if(continuar) {
				if((angulo == anguloUltimoVerde && verde < 420 && verde > 220 && ultimoLadoVerde == 1 && (anguloInicialUltimoVerde == 0 || anguloInicialUltimoVerde == 90 || anguloInicialUltimoVerde == 180 || anguloInicialUltimoVerde == 270)) || (ladrilhoQuadrado && ultimoLadoVerde == 1 && angulo == anguloInicialUltimoVerde && preto > 1500 && preto < 2200)) {
					Gap(-73);
				} else {
					varTempo = bc.Timer();
					GiroPreto(2, 3000);
					if(continuar) {
						varTempo = bc.Timer();
						GiroPreto(1, 5400);
					}
				}
				bc.MoveFrontal(0, 0);
				if(continuar) {
					if(ultimoLadoVerde == 1) {
						Gap(-163);
					} else {
						Gap(17);
					}
				}
			}
		} else {
			GiroPreto(2, 300);
			if(continuar) {
				if((angulo == anguloUltimoVerde && verde < 420 && verde > 220 && ultimoLadoVerde == 2 && (anguloInicialUltimoVerde == 0 || anguloInicialUltimoVerde == 90 || anguloInicialUltimoVerde == 180 || anguloInicialUltimoVerde == 270)) || (ladrilhoQuadrado && ultimoLadoVerde == 2 && angulo == anguloInicialUltimoVerde && preto > 1500 && preto < 2200)) {
					Gap(73);
				} else {
					varTempo = bc.Timer();
					GiroPreto(1, 3000);
				}
				if(continuar) {
					varTempo = bc.Timer();
					GiroPreto(2, 5400);
				}
			}
			bc.MoveFrontal(0, 0);
			if(continuar) {
				if(ultimoLadoVerde == 2) {
					Gap(163);
				} else {
					Gap(-17);
				}
			}
		}
		if(!continuar) {
			if(bc.ReturnColor(0) == "PRETO") {
				while(bc.ReturnColor(0) == "PRETO") {
					if(bc.ReturnColor(1) == "PRETO") {
						ultimoLadoDoisPretos = 1;
						break;
					}
					bc.MoveFrontal(-1000, 1000);
				}
				bc.MoveFrontal(-1000, 1000);
				bc.Wait(100);
			} else {
				while(bc.ReturnColor(1) == "PRETO") {
					if(bc.ReturnColor(0) == "PRETO") {
						ultimoLadoDoisPretos = 2;
						break;
					}
					bc.MoveFrontal(1000, -1000);
				}
				bc.MoveFrontal(1000, - 1000);
				bc.Wait(100);
			}
		}
		tempoUltimoDoisPretos = bc.Timer();
	}
}

void Verde() {
	if(bc.ReturnGreen(0) > bc.ReturnRed(0) + 15 || bc.ReturnColor(0) == "VERDE") {
		bc.Wait(10);
		if(bc.ReturnGreen(0) > bc.ReturnRed(0) + 15 || bc.ReturnColor(0) == "VERDE") {
			bc.MoveFrontal(100, 100);
			bc.Wait(50);
			if(bc.ReturnGreen(1) > bc.ReturnRed(1) + 15 || bc.ReturnColor(1) == "VERDE") {
				DoisVerdes();
			} else {
				GiroVerde(10, -1000, 0);
			}
		}
	} else if(bc.ReturnGreen(1) > bc.ReturnRed(1) + 15 || bc.ReturnColor(1) == "VERDE") {
		bc.Wait(10);
		if(bc.ReturnGreen(1) > bc.ReturnRed(1) + 15 || bc.ReturnColor(1) == "VERDE") {
			bc.MoveFrontal(100, 100);
			bc.Wait(50);
			if(bc.ReturnGreen(0) > bc.ReturnRed(0) + 15 || bc.ReturnColor(0) == "VERDE") {
				DoisVerdes();
			} else {
				GiroVerde(-10, 1000, 1);
			}
		}
	}
}

void Preto() {
	if(bc.Lightness(0) < Calibracao1() || bc.ReturnColor(0) == "PRETO" || bc.Lightness(0) < 50) {
		if(bc.Lightness(1) < Calibracao2() || bc.ReturnColor(1) == "PRETO" || bc.Lightness(1) < 50) {
			DoisPretos();
		} else {
			bc.MoveFrontal(-1000, 1000);
			bc.Wait(16);
			varTempoDir = bc.Timer();
			vezesGiroDir += 1;
			if(bc.Lightness(1) < Calibracao2() || bc.ReturnColor(1) == "PRETO" || bc.Lightness(1) < 50) {
				DoisPretos();
			}
		}
	} else if(bc.Lightness(1) < Calibracao2() || bc.ReturnColor(1) == "PRETO" || bc.Lightness(1) < 50) {
		if(bc.Lightness(0) < Calibracao1() || bc.ReturnColor(0) == "PRETO" || bc.Lightness(0) < 50) {
			DoisPretos();
		} else {
			bc.MoveFrontal(1000, -1000);
			bc.Wait(16);
			varTempoEsq = bc.Timer();
			vezesGiroEsq += 1;
			if(bc.Lightness(0) < Calibracao1() || bc.ReturnColor(0) == "PRETO" || bc.Lightness(0) < 50) {
				DoisPretos();
			}
		}
	} else {
		diferenca = Diferenca();
		valorTendencioso = (float)(300-((-1.8*diferenca)+211));
		if(varTempoEsq > varTempoDir && diferenca > 1) {
			bc.MoveFrontal(300, -valorTendencioso);
			bc.Wait(16);
			if(Diferenca() > diferenca) {
				bc.MoveFrontal(-valorTendencioso, 300);
				bc.Wait(16);
				varTempoDir = bc.Timer();
			}
		} else if(varTempoDir > varTempoEsq && diferenca > 1) {
			bc.MoveFrontal(-valorTendencioso, 300);
			bc.Wait(16);
			if(Diferenca() > diferenca) {
				bc.MoveFrontal(300, -valorTendencioso);
				bc.Wait(16);
				varTempoEsq = bc.Timer();
			}
		} else {
			bc.MoveFrontal(velocidade, velocidade);
		}
	}
	if(bc.Timer()-varTempoDir > 300) {
		vezesGiroDir = 0;
	} else if(bc.Timer()-vezesGiroEsq > 300) {
		vezesGiroEsq = 0;
	}
}

void Desvio() {
	if(bc.Distance(0) < 70) {
		while(true) {
			if(bc.Distance(0) > 75) {
				bc.MoveFrontal(0, 0);
				desvio = false;
				break;
			} else if(bc.Distance(0) < 32) {
				bc.MoveFrontal(0, 0);
				desvio = true;
				break;
			} else {
				velocidade = (bc.Distance(0) * 3);
				Preto();
				Verde();
			}
		}
		if(desvio) {
			Girar(45);
			desvio = true;
			varTempo = bc.Timer();
			while(bc.Timer() - varTempo < 680) {
				if(bc.Lightness(1) < 40) {
					desvio = false;
					break;
				} else {
					bc.MoveFrontal(180, 180);
				}
			}
			if(desvio) {
				Girar(-45);
				bc.MoveFrontalRotations(180, 18);
				Girar(-45);
				varTempo = bc.Timer();
				while(bc.Timer() - varTempo < 480) {
					if(bc.Lightness(1) < 40) {
						desvio = false;
						break;
					} else {
						bc.MoveFrontal(180, 180);
					}
				}
				if(desvio) {
					Girar(-45);
					bc.MoveFrontalRotations(180, 10);
					Girar(-45);
					while(bc.Lightness(1) > 40) {
						bc.MoveFrontal(180, 180);
					}
				}
			}
			bc.MoveFrontalRotations(180, 14);
			Girar(45);
			varTempo = bc.Timer();
			while(!bc.Touch(0)) {
				if(bc.Timer()-varTempo > 450) {
					break;
				} else {
					bc.MoveFrontal(-180, -180);
				}
			}
			bc.MoveFrontal(0, 0);
		}
		velocidade = 130;
		Verde();
	}
}

void SeguidorRampa() {
	if(bc.ReturnColor(0) == "PRETO") {
		bc.MoveFrontal(-1000, 1000);
	} else if(bc.ReturnColor(1) == "PRETO") {
		bc.MoveFrontal(1000, -1000);
	} else {
		bc.MoveFrontal(velocidade, velocidade);
	}
}

void SeguidorRedzone() {
	while(true) {
		if(bc.ReturnGreen(0) > bc.ReturnRed(0) + 10 || bc.ReturnGreen(1) > bc.ReturnRed(1) + 10 || bc.ReturnColor(0) == "VERDE" || bc.ReturnColor(1) == "VERDE") {
			break;
		}
		bc.MoveFrontal(150, 150);
	}
	while(bc.ReturnGreen(0) > bc.ReturnRed(0) + 10 || bc.ReturnGreen(1) > bc.ReturnRed(1) + 10 || bc.ReturnColor(0) == "VERDE" || bc.ReturnColor(1) == "VERDE") {
		bc.MoveFrontal(100, 100);
	}
	bc.Wait(100);
	velocidade = 130;
	varTempo = 0;
	varTempoEsq = 0;
	varTempoDir = 0;
	ultimoLadoVerde = 0;
	tempoUltimoVerde = 0;
	anguloUltimoVerde = 0;
	anguloInicialUltimoVerde = 0;
	tempoUltimoDoisPretos = 0;
	ladrilhoQuadrado = false;
	redzoneCompleta = true;
	bc.ResetTimer();
	while(true) {
		Verde();
		Preto();
		Desvio();
		Rampa();
		if(bc.ReturnRed(0) > bc.ReturnGreen(0) + 15 || bc.ReturnRed(1) > bc.ReturnGreen(1) + 15 || bc.ReturnColor(0) == "VERMELHO" || bc.ReturnColor(1) == "VERMELHO") {
			bc.Wait(10);
			if(bc.ReturnRed(0) > bc.ReturnGreen(0) + 15 || bc.ReturnRed(1) > bc.ReturnGreen(1) + 15 || bc.ReturnColor(0) == "VERMELHO" || bc.ReturnColor(1) == "VERMELHO") {
				Alinhar(true);
				bc.MoveFrontal(300, 300);
				bc.Wait(350);
				bc.MoveFrontal(0, 0);
				while(true) {
					bc.Wait(1000);
				}
			}
		}
	}
}

void Alinhar(bool verAngulo) {
	if(Diferenca() <= 4 && verAngulo) {
		return;
	} else {
		float angulo = Angulo();
		if(bc.Compass() >= 315) {
			while(bc.Compass() < 359) {
				bc.MoveFrontal(-1000, 1000);
			}
		} else {
			if(bc.Compass() > angulo) {
				if(angulo == 0) {
					angulo = 1;
				}
				while(bc.Compass() > angulo) {
					bc.MoveFrontal(1000, -1000);
				}
			} else {
				if(angulo == 0) {
					angulo = 359;
				}
				while(bc.Compass() < angulo) {
					bc.MoveFrontal(-1000, 1000);
				}
			}
		}
		bc.MoveFrontal(0, 0);
	}
}

void baixaGarra() {
	while(bc.AngleActuator() > 2) {
		bc.ActuatorDown(1);
	}
	bc.ActuatorDown(40);
	bc.Wait(40);
}

void levantaGarra() {
	bc.ActuatorSpeed(150);
	bc.ActuatorUp(30);
	bc.CloseActuator();
	bc.Wait(40);
	while(bc.AngleActuator() < 88) {
		bc.ActuatorUp(1);
	}
	bc.ActuatorUp(10);
}

void BuscarBolinha() {
	bc.MoveFrontal(0, 0);
	Girar(90);
	if(ultrassonicoFrente > 267) {
		bc.MoveFrontal(-1000, 1000);
		bc.Wait(30);
		bc.MoveFrontal(0, 0);
	}
	if(ultrassonicoBolinha < 45) {
		if(((casoEspecial == 2 || casoEspecial == 5) && posicaoTriangulo == 3) || (casoEspecial == 1 && posicaoTriangulo == 1)) {
			bc.MoveFrontal(-130, -130);
			bc.Wait((int)(-15.0 * ultrassonicoBolinha + 820));
			bc.MoveFrontal(0, 0);
		} else {
			bc.MoveFrontal(100, 100);
			bc.Wait(975);
			bc.MoveFrontal(0, 0);
			bc.Wait(150);
			bc.MoveFrontal(-300, -300);
			bc.Wait(280);
			bc.MoveFrontal(0, 0);
		}
	}
	bc.ResetTimer();
	while(true) {
		bc.MoveFrontal(300, 300);
		if(ultrassonicoBolinha < 50 || ((bc.Timer() > (11*ultrassonicoBolinha)-1284) && ultrassonicoBolinha >= 135) || (bc.Timer() > ((10*ultrassonicoBolinha)-170)-250 && ultrassonicoBolinha < 135)) {
			break;
		}
	}
	int frenteRapido = bc.Timer();
	if(ultrassonicoBolinha < 135) {
		bc.MoveFrontal(0, 0);
	}
	bc.OpenActuator();
	baixaGarra();
	bc.ResetTimer();
	while(true) {
		if(bc.HasVictim() || bc.Distance(0) < 55 || (bc.Timer()/2)+frenteRapido > 2300 || Diferenca() > 15) {
			break;
		} else if(bc.Inclination() < 350 && bc.Inclination() > 330) {
			bc.Wait(32);
			if(bc.Inclination() < 350 && bc.Inclination() > 330) {
				bc.MoveFrontal(-300, -300);
				bc.Wait(450);
			}
		} else if((bc.Timer() > 650 && casoEspecial == 1 && posicaoTriangulo == 1)) {
			break;
		}
		if(!bolinhaGrudada) {
			bc.MoveFrontal(150, 150);
		} else {
			bc.MoveFrontal(300, 300);
		}
	}
	bc.MoveFrontal(200, 200);
	levantaGarra();
	bolinhaGrudada = false;
	int frenteDevagar = bc.Timer();
	Alinhar(true);
	if(posicaoTriangulo == 1 && casoEspecial == 1) {
		posicaoTriangulo = 2;
		casoEspecial = 3;
	}
	bc.ResetTimer();
	bool pegouBolinha = bc.HasVictim();
	if(bc.HasVictim()) {
		if(posicaoTriangulo == 3) {
			while(true) {
				if(bc.Touch(0) || ((casoEspecial == 2 || casoEspecial == 5) && bc.Distance(0) > 200 && bc.Distance(0) < 400) || (bc.Distance(0) > 230 && bc.Distance(0) < 400)) {
					bc.Wait(10);
					if(bc.Touch(0) || ((casoEspecial == 2 || casoEspecial == 5) && bc.Distance(0) > 200 && bc.Distance(0) < 400) || (bc.Distance(0) > 230 && bc.Distance(0) < 400)) {
						break;
					}
				} else {
					bc.MoveFrontal(-300, -300);
				}
			}
			if(bc.Touch(0)) {
				bc.MoveFrontal(300, 300);
				bc.Wait(380);
			}
			if(casoEspecial == 2 || casoEspecial == 5) {
				if(casoEspecial == 2) {
					Girar(-90);
				} else {
					Girar(90);
				}
				if(bc.Distance(0) < 65) {
					while(bc.Distance(0) < 65) {
						bc.MoveFrontal(-300, -300);
					}
				} else {
					while(bc.Distance(0) > 65) {
						bc.MoveFrontal(300, 300);
					}
				}
				posicaoTriangulo = 2;
			}
		} else if(posicaoTriangulo == 2 && casoEspecial == 3) {
			if(bc.Distance(0) < 300) {
				while(bc.Distance(0) < 50) {
					bc.MoveFrontal(-300, -300);
				}
			} else {
				bc.MoveFrontal(-300, -300);
				bc.Wait(frenteRapido+frenteDevagar-350);
			}
		} else {
			if(posicaoTriangulo == 1) {
				distanciaEntrega = (-ultrassonicoFrente) + 269;
			} else {
				distanciaEntrega = ultrassonicoFrente-10;
			}
			if(distanciaEntrega > 230) {
				distanciaEntrega = 230;
			}
			if(bc.Distance(0) < distanciaEntrega) {
				while(bc.Distance(0) < distanciaEntrega) {
					bc.MoveFrontal(-300, -300);
				}
			} else if(bc.Distance(0) > 400 && posicaoTriangulo == 2) {
				while(!bc.Touch(0)) {
					bc.MoveFrontal(-300, -300);
				}
				bc.MoveFrontal(300, 300);
				bc.Wait(380);
			} else {
				float luzInicial = bc.Lightness(2);
				while(bc.Distance(0) > distanciaEntrega) {
					if((posicaoTriangulo == 1 && ultrassonicoFrente > 200) || (posicaoTriangulo == 2 && ultrassonicoFrente < 65)) {
						if((bc.ReturnColor(2) == "PRETO" || bc.Lightness(2) < luzInicial-5 || bc.Lightness(2) > luzInicial+5 || Diferenca() > 15) && bc.Distance(0) < 100) {
							break;
						}
					}
					if(bc.Inclination() < 350 && bc.Inclination() > 330) {
						bc.Wait(20);
						if(bc.Inclination() < 350 && bc.Inclination() > 330) {
							bc.MoveFrontal(-300, -300);
							bc.Wait(500);
						}
					}
					bc.MoveFrontal(300, 300);
				}
			}
		}
		if(posicaoTriangulo == 1 || (posicaoTriangulo == 2 && casoEspecial == 5)) {
			Girar(45);
		} else if(posicaoTriangulo == 2 && casoEspecial != 3) {
			Girar(-45);
		} else {
			Girar(-90);
		}
		float luz = bc.Lightness(2);
		while(true) {
			if((bc.ReturnColor(2) == "PRETO" || bc.Lightness(2) < luz-5 || bc.Lightness(2) > luz+5 || Diferenca() > 15) && bc.Distance(0) < 100) {
				break;
			}
			bc.MoveFrontal(300, 300);
		}
		if(posicaoTriangulo == 3) {
			Girar(-45);
		} else if(posicaoTriangulo == 2 && casoEspecial == 3) {
			Girar(45);
		}
		bc.MoveFrontal(150, 150);
		bc.Wait(300);
		bc.MoveFrontal(0, 0);
		baixaGarra();
		bc.TurnActuatorDown(100);
		bc.Wait(75);
		bc.MoveFrontal(300, 300);
		bc.Wait(50);
		bc.MoveFrontal(0, 0);
		while(bc.HasVictim()) {
			bc.Wait(100);
		}
		bc.TurnActuatorUp(100);
		levantaGarra();
		bc.MoveFrontal(-50, -50);
		bc.Wait(200);
		if(posicaoTriangulo == 1 || (posicaoTriangulo == 2 && casoEspecial == 5)) {
			Girar(-45);
			if(bc.Inclination() > 300) {
				bc.MoveFrontal(0, 0);
				bc.Wait(100);
				if(bc.Inclination() > 300) {
					bc.MoveFrontal(-300, -300);
					bc.Wait(1000);
				}
			}
		} else {
			if(casoEspecial != 3 || (posicaoTriangulo == 3 && casoEspecial == 3)) {
				Girar(45);
				Girar(45);
				while(bc.Distance(0) < 150) {
					bc.MoveFrontal(-300, -300);
				}
			}
			Girar(-45);
		}
	} else {
		if(posicaoTriangulo == 3 || (posicaoTriangulo == 2 && casoEspecial == 1)) {
			if(bc.Distance(0) < 300) {
				while(bc.Distance(0) < 250) {
					bc.MoveFrontal(-300, -300);
				}
			} else {
				while(!bc.Touch(0)) {
					bc.MoveFrontal(-300, -300);
				}
				bc.MoveFrontal(300, 300);
				bc.Wait(150);
			}
			Girar(-90);
		} else if(posicaoTriangulo == 2 && casoEspecial == 3) {
			if(bc.Distance(0) < 300) {
				while(bc.Distance(0) < 50) {
					bc.MoveFrontal(-300, -300);
				}
			} else {
				bc.MoveFrontal(-300, -300);
				bc.Wait(500);
			}
			Girar(-90);
		}
	}
	if(posicaoTriangulo == 2 && (casoEspecial == 3 || casoEspecial == 5)) {
		posicaoTriangulo = 1;
		casoEspecial = 0;
	} else if(posicaoTriangulo == 3 && casoEspecial == 3) {
		posicaoTriangulo = 2;
		casoEspecial = 0;
	}
	if(posicaoTriangulo == 1 || (posicaoTriangulo == 2 && casoEspecial == 0)) {
		bc.MoveFrontal(-300, -300);
		bc.Wait(100);
		while(bc.Distance(0) < 255) {
			ultrassonico = bc.Distance(1);
			bc.MoveFrontal(-300, -300);
			bc.Wait(25);
			if((bc.Distance(1) < ultrassonico-5 && pegouBolinha) || (bc.Distance(1)-ultrassonico > -1 && bc.Distance(1)-ultrassonico < 1 && posicaoTriangulo == 1 && bc.Distance(0) < 88)) {
				if(posicaoTriangulo == 1) {
					casoEspecial = 1;
					break;
				} else if(posicaoTriangulo == 2 && (bc.Distance(0) < 160 || (bc.Distance(0) >= 160 && bc.Distance(1) < 75))) {
					casoEspecial = 4;
					break;
				}
			} else if(bc.Touch(0) && bc.Distance(0) > 245) {
				break;
			}
		}
		if(casoEspecial != 1 && casoEspecial != 4) {
			bc.MoveFrontal(0, 0);
			bc.MoveFrontal(1000, -1000);
			bc.Wait(50);
			Girar(-90);
		}
	}
	ultrassonico = bc.Distance(1);
	if(casoEspecial != 1) {
		if(posicaoTriangulo == 1) {
			bc.MoveFrontal(300, 300);
		} else {
			bc.MoveFrontal(-300, -300);
		}
		bc.Wait(150);
	}
	if(((casoEspecial == 1 && pegouBolinha) || casoEspecial == 2) && posicaoTriangulo == 2) {
		posicaoTriangulo = 3;
		casoEspecial = 0;

	} else if(posicaoTriangulo == 2 && casoEspecial == 3) {
		posicaoTriangulo = 1;
		casoEspecial = 0;
	}
}

void ProcuraBolinha() {
	ultrassonico = 0;
	while(true) {
		while(true) {
			ultrassonico = bc.Distance(1);
			if(posicaoTriangulo == 1 && casoEspecial != 1) {
				bc.MoveFrontal(300, 300);
			} else {
				bc.MoveFrontal(-300, -300);
			}
			bc.Wait(25);
			if(bc.Distance(1) - ultrassonico > -1 && bc.Distance(1) - ultrassonico < 1 && ((bc.Distance(0) > 183 && posicaoTriangulo == 1) || (bc.Distance(0) < 80 && posicaoTriangulo == 2))) {
				bolinhaGrudada = true;
				ultrassonicoBolinha = bc.Distance(1);
				break;
			}
			if(bc.Distance(1) < ultrassonico - 5 || bc.Distance(1) < 150 || (casoEspecial == 1 && posicaoTriangulo == 1) || (casoEspecial == 4 && posicaoTriangulo == 2)) {
				ultrassonicoBolinha = bc.Distance(1);
				break;
			} else if(bc.Distance(0) < 30 && posicaoTriangulo == 1) {
				bc.MoveFrontal(0, 0);
				while(bc.Distance(0) < 38) {
					bc.MoveFrontal(-300, -300);
				}
				Girar(-90);
				if(bc.Distance(0) < 100) {
					while(bc.Distance(0) < 235) {
						ultrassonico = bc.Distance(1);
						bc.MoveFrontal(-300, -300);
						bc.Wait(25);
						if(bc.Distance(1) < ultrassonico - 5) {
							posicaoTriangulo = 3;
							casoEspecial = 5;
							break;
						}
					}
					if(bc.Distance(0) >= 235) {
						Girar(90);
					}
				}
				if(bc.Distance(0) >= 235) {
					SeguidorRedzone();
				}
			} else if(posicaoTriangulo == 2 && ((bc.Distance(0) > 270) || (bc.Touch(0) && bc.Distance(0) > 250))) {
				if(casoEspecial == 0) {
					bc.MoveFrontal(100, 100);
					bc.Wait(350);
					Alinhar(false);
					bc.MoveFrontal(-1000, 1000);
					bc.Wait(30);
					if(probabilidadeSaida != 0) {
						while(bc.Distance(0) > 240) {
							bc.MoveFrontal(300, 300);
						}
						Girar(90);
						bc.ResetTimer();
						while(bc.Timer() <= 2500) {
							ultrassonico = bc.Distance(1);
							bc.MoveFrontal(300, 300);
							bc.Wait(25);
							if(bc.Distance(1) < ultrassonico-5 && ultrassonico < 300) {
								posicaoTriangulo = 3;
								posicaoTrianguloOriginal = 2;
								casoEspecial = 2;
								break;
							}
						}
						if(bc.Timer() >= 2500) {
							SeguidorRedzone();
						}
					} else {
						while(bc.Distance(0) > 45) {
							bc.MoveFrontal(300, 300);
						}
						Girar(-90);
						SeguidorRedzone();
					}
				} else {
					bc.MoveFrontal(0, 0);
					if(bc.Distance(1) > 400) {
						while(bc.Distance(0) > 245) {
							bc.MoveFrontal(300, 300);
						}
						Girar(90);
						bc.ResetTimer();
						while(bc.Timer() < 2500) {
							ultrassonico = bc.Distance(1);
							bc.MoveFrontal(300, 300);
							bc.Wait(25);
							if(bc.Distance(1) < ultrassonico - 5) {
								posicaoTriangulo = 3;
								casoEspecial = 2;
								break;
							}
						}
						if(bc.Timer() >= 2500) {
							SeguidorRedzone();
						}
					} else {
						while(bc.Distance(0) > 260) {
							bc.MoveFrontal(300, 300);
						}
						Girar(90);
						while(bc.Distance(0) > 245) {
							bc.MoveFrontal(300, 300);
						}
						Girar(90);
						SeguidorRedzone();
					}
				}
			} else if(posicaoTriangulo == 3 && ((bc.Distance(0) > 270) || (bc.Touch(0) && bc.Distance(0) > 250))) {
				bc.MoveFrontal(100, 100);
				bc.Wait(350);
				Alinhar(false);
				while(bc.Distance(0) > 260) {
					bc.MoveFrontal(300, 300);
				}
				Girar(-90);
				posicaoTriangulo = 2;
				if(posicaoTrianguloOriginal != 2) {
					casoEspecial = 1;
				} else {
					casoEspecial = 0;
				}
				bc.MoveFrontal(-300, -300);
				bc.Wait(100);
			}
		}
		while(true) {
			if((posicaoTriangulo == 1 && casoEspecial != 1) || (posicaoTriangulo == 3 && casoEspecial == 2)) {
				bc.MoveFrontal(300, 300);
			} else {
				bc.MoveFrontal(-300, -300);
			}
			if(bc.Distance(1) < ultrassonicoBolinha-5) {
				ultrassonicoBolinha = bc.Distance(1);
				bolinhaGrudada = false;
			} else if(bc.Distance(1) > ultrassonicoBolinha + 5 || bc.Distance(0) < 30) {
				break;
			} else if(bc.Touch(0) || bc.Distance(0) > 267) {
				break;
			}
		}
		bc.ResetTimer();
		while(bc.Distance(1) > ultrassonicoBolinha+5 || bc.Distance(1) < ultrassonicoBolinha-5) {
			if((posicaoTriangulo == 1 && casoEspecial != 1) || (posicaoTriangulo == 3 && casoEspecial == 2)) {
				bc.MoveFrontal(-300, -300);
			} else {
				bc.MoveFrontal(300, 300);
			}
			if(bc.Timer() > 500) {
				buscarBolinha = false;
				break;
			}
		}
		if(buscarBolinha) {
			ultrassonicoFrente = bc.Distance(0);
			if(bc.Distance(0) > 262 && bc.Distance(0) < 300) {
				while(bc.Distance(0) > 262 && bc.Distance(0) < 300) {
					bc.MoveFrontal(100, 100);
				}
			}
			if(posicaoTriangulo == 1 || (posicaoTriangulo == 3 && casoEspecial == 2)) {
				bc.MoveFrontal(-300, -300);
				if(ultrassonicoBolinha >= 50) {
					bc.Wait(60);
				} else if(ultrassonicoBolinha <= 13) {
					bc.Wait(200);
				} else {
					bc.Wait(120);
				}
			} else {
				if(ultrassonicoBolinha > 13) {
					bc.MoveFrontal(300, 300);
					bc.Wait(30);
				} else {
					bc.MoveFrontal(-300, -300);
					bc.Wait(250);
				}
			}
			if(posicaoTriangulo == 2 && casoEspecial == 4) {
				posicaoTriangulo = 3;
				casoEspecial = 3;
			}
			BuscarBolinha();
		} else {
			buscarBolinha = true;
		}
	}
}

void Redzone() {
	velocidade = 200;
	bc.ResetTimer();
	while(bc.Distance(1) < 55) {
		if(velocidade == 200 && bc.Timer() > 2700) {
			velocidade = 150;
		}
		SeguidorRampa();
	}
	bc.Wait(5);
	bc.MoveFrontal(0, 0);
	bc.Wait(20);
	if(bc.Distance(1) < 140 && bc.Distance(1) > 134) {
		probabilidadeTriangulo = 10;
	}
	Girar(-45);
	while(bc.Distance(0) > 24) {
		bc.MoveFrontal(250, 250);
	}
	Girar(45);
	bc.ResetTimer();
	bool bolinhaPrimeiroCaso = false;
	while(bc.Distance(0) > 180) {
		ultrassonico = bc.Distance(1);
		bc.MoveFrontal(300, 300);
		bc.Wait(16);
		if(bc.Distance(1) > 300) {
			probabilidadeSaida = probabilidadeSaida + 1;
		} else if(bc.Distance(1) - ultrassonico > 0.5 && bc.Distance(1) - ultrassonico < 2 && bc.Distance(1) > 165) {
			probabilidadeTriangulo = probabilidadeTriangulo + 1;
		} else if((bc.Distance(1) - ultrassonico < -3 || bc.Distance(1) < 160) && probabilidadeSaida < 2 && bc.Distance(0) < 240 && !bolinhaPrimeiroCaso) {
			ultrassonicoFrente = bc.Distance(0);
			bolinhaPrimeiroCaso = true;
		}
	}
	bc.MoveFrontal(0, 0);
	if(probabilidadeTriangulo > 5 || posicaoTriangulo == 1) {
		posicaoTriangulo = 1;
		if(bolinhaPrimeiroCaso) {
			while(bc.Distance(0) < ultrassonicoFrente-5) {
				bc.MoveFrontal(-300, -300);
			}
			ultrassonicoFrente = bc.Distance(0);
			ultrassonicoBolinha = bc.Distance(1);
			BuscarBolinha();
		}
		ProcuraBolinha();
	}
	while(true) {
		bc.MoveFrontal(300, 300);
		ultrassonico = bc.Distance(0);
		bc.Wait(75);
		if(ultrassonico - bc.Distance(0) < 2 && bc.ReturnColor(2) == "PRETO") {
			posicaoTriangulo = 3;
			break;
		} else if(bc.Distance(0) < 25) {
			posicaoTriangulo = 2;
			break;
		}
	}
	Alinhar(true);
	if(bc.Distance(1) > 200 && posicaoTriangulo == 2) {
		while(bc.Distance(0) < 240) {
			bc.MoveFrontal(-300, -300);
		}
		posicaoTriangulo = 1;
		bc.MoveFrontal(300, 300);
		bc.Wait(100);
	} else {
		bc.MoveFrontal(-300, -300);
		bc.Wait(100);
	}
	ProcuraBolinha();
}

void Rampa() {
	if(bc.Inclination() < 341 && bc.Inclination() > 330) {
		varTempo = bc.Timer();
		while(bc.Timer()-varTempo < 100) {
			Preto();
			Verde();
			Desvio();
		}
		if(bc.Inclination() < 341 && bc.Inclination() > 330) {
			velocidade = 150;
			varTempo = bc.Timer();
			while(true) {
				Preto();
				Verde();
				Desvio();
				if(bc.Inclination() > 350 || bc.Inclination() < 30) {
					varTempo = bc.Timer();
					while(bc.Timer()-varTempo < 800) {
						Preto();
						Verde();
						Desvio();
						if(bc.Inclination() > 8 && bc.Inclination() < 30) {
							bc.MoveFrontal(150, 150);
							bc.Wait(300);
							break;
						}
					}
					if(bc.Inclination() > 8 && bc.Inclination() < 30) {
						Alinhar(true);
					}
					break;
				} else if(bc.Distance(1) < 50 && bc.Distance(1) > 20 &&  bc.Timer()-varTempo < 200 && !redzoneCompleta) {
					bc.Wait(100);
					if(bc.Distance(1) < 50 && bc.Distance(1) > 20) {
						Redzone();
					}
				} else if(bc.Inclination() > 342 && bc.Timer()-varTempo < 1700) {
					bc.MoveFrontal(100, 100);
					bc.Wait(200);
					bc.MoveFrontal(0, 0);
					bc.Wait(2500);
					Alinhar(true);
					break;
				}
			}
			velocidade = 130;
		}
	}
}

void Main() {
	bc.ResetTimer();
	levantaGarra();
	while(true) {
		Rampa();
		Verde();
		Preto();
		Desvio();
	}
}
