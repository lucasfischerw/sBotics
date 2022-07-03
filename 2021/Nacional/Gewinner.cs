int VarTempo = 0;
int VarTempoDir = 0;
int VarTempoEsq = 0;
int ContagemDir = 0;
int ContagemEsq = 0;
int ContagemParado = 0;
int LadoDoisPretos = 0;
int TempoUltimoDoisPretos = 0;
int VarTempoTendencioso = -89;
float Velocidade = 130;
float[] Luz = {bc.Lightness(0), bc.Lightness(1)};
float ValorTendencioso = 0;
bool DesvioVerdadeiro = false;
bool LadrilhoQuadrado =  false;
bool ContinuarDoisPretos = false;
int[] InformacoesUltimoVerde = {0, 0, 0, 0, 0};
int[][] AngulosRetos = {new []{0, 90, 180, 270, 360}, new []{0, 45, 90, 135, 180, 225, 270, 315, 360}};

int TempoTras = 0;
int TempoFrente = 0;
int PosicaoTriangulo = 0;
int ProbabilidadeTriangulo = 0;
int PosicaoTrianguloOriginal = 0;
float Ultrassonico = 0;
float UltrassonicoFrente = 0;
float UltrassonicoBolinha = 0;
float MaiorDistanciaPercorrida = 0;
bool RedzoneCompleta = false;
bool ResgatouBolinha = false;
bool IrBuscarBolinha = true;
int[] ValoresRedzone = {0, 0, 0, 0};
static int VitimasVivasResgatadas = 0;

int Angulo(int Precisao) {
	float Direcao = bc.Compass();
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

float Diferenca(int Precisao) => bc.Compass()-Angulo(Precisao);

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

void MoverTempo(float VelDir, float VelEsq, int Tempo, bool Parar) {
	bc.Move(VelEsq, VelDir);
	bc.Wait(Tempo);
	if(Parar) {
		bc.Move(0, 0);
	}
}

void Girar(int Angulo, int Precisao) {
	bc.Move(1000*(Angulo/Math.Abs(Angulo)), -1000*(Angulo/Math.Abs(Angulo)));
	bc.Wait(100);
	int VarObjetivo = Objetivo(Angulo, Precisao);
	if(bc.Compass() > VarObjetivo && Angulo >= 0) {
		while(bc.Compass() > VarObjetivo) {
			bc.Wait(1);
		}
		bc.Wait(100);
	} else if(bc.Compass() < VarObjetivo && Angulo < 0) {
		while(bc.Compass() < VarObjetivo) {
			bc.Wait(1);
		}
		bc.Wait(100);
	}
	while((bc.Compass() > VarObjetivo && Angulo < 0) || (bc.Compass() < VarObjetivo && Angulo > 0)) {
		bc.Wait(1);
	}
	bc.Move(0, 0);
}

void GiroVerde(int Lado, int Giro, int Sensor1, int Sensor2) {
	int AnguloInicialVerde = Angulo(1);
	int ObjetivoTempo = 0;
	bool Preto = false;
	bool DoisVerdes = false;
	if(Angulo(0) == Angulo(1)) {
		ObjetivoTempo = 450;
	} else {
		ObjetivoTempo = 350;
	}
	VarTempo = bc.Timer();
	while(bc.Timer()-VarTempo < ObjetivoTempo) {
		bc.Move(200, 200);
		if((bc.ReturnGreen(Sensor2) > bc.ReturnBlue(Sensor2)+45 || bc.ReturnColor(Sensor2) == "VERDE") && !Preto) {
			bc.Wait(10);
			if((bc.ReturnGreen(Sensor2) > bc.ReturnBlue(Sensor2)+45 || bc.ReturnColor(Sensor2) == "VERDE") && !Preto) {
				DoisVerdes = true;
				break;
			}
		} else if(bc.Lightness(Sensor2) < 35) {
			Preto = true;
		}
	}
	if(!DoisVerdes && ObjetivoTempo == 450) {
		bc.MoveFrontalAngles(1000, Lado);
	} else if(DoisVerdes) {
		bc.MoveFrontalRotations(200, 14);
		bc.MoveFrontalAngles(1000, 120);
		Sensor1 = 0;
		Giro = -1000;
	}
	while(bc.ReturnColor(Sensor1) == "PRETO") {
		bc.Move(-Giro, Giro);
	}
	while(bc.ReturnColor(Sensor1) != "PRETO") {
		bc.Move(-Giro, Giro);
		if(Math.Abs(Diferenca(0)) < 15) {
			Prata();
		}
	}
	while(bc.ReturnColor(Sensor1) == "PRETO") {
		if(bc.ReturnColor(Sensor2) == "PRETO" && !DoisVerdes) {
			MoverTempo(-Giro, Giro, 70, false);
			MoverTempo(-130, -130, 200, true);
			break;
		} else {
			bc.Move(-Giro, Giro);
		}
	}
	if(Diferenca(0) > 0) {
		VarTempoEsq = bc.Timer();
	} else {
		VarTempoDir = bc.Timer();
	}
	bc.Wait(70);
	bc.Move(0, 0);
	if(!DoisVerdes) {
		InformacoesUltimoVerde = new int[]{AnguloInicialVerde, Sensor1+1, Angulo(0), bc.Timer()};
	}
	LadrilhoQuadrado = false;
	Luz = new float[] {bc.Lightness(0), bc.Lightness(1)};
}

void GiroPreto(int Lado, int ObjetivoTempo, int ObjetivoGiroLadrilhoQuadrado) {
	int ObjetivoGirar = Objetivo(ObjetivoGiroLadrilhoQuadrado, 0);
	VarTempo = bc.Timer();
	LadoDoisPretos = Lado;
	while(true) {
		if(bc.Timer()-VarTempo > ObjetivoTempo) {
			ContinuarDoisPretos = true;
			break;
		} else if((bc.Compass()-ObjetivoGirar > 0 && bc.Compass()-ObjetivoGirar < 10 && Lado == 1 && LadrilhoQuadrado && ObjetivoGiroLadrilhoQuadrado != 1 && (InformacoesUltimoVerde[1] == 2 || ObjetivoGiroLadrilhoQuadrado != 73)) || (bc.Compass()-ObjetivoGirar < 0 && bc.Compass()-ObjetivoGirar > -10 && Lado == -1 && LadrilhoQuadrado && ObjetivoGiroLadrilhoQuadrado != 1 && (InformacoesUltimoVerde[1] == 1 || ObjetivoGiroLadrilhoQuadrado != -73))) {
			bc.Wait(25);
			if((bc.Compass()-ObjetivoGirar > 0 && bc.Compass()-ObjetivoGirar < 10 && Lado == 1 && LadrilhoQuadrado && ObjetivoGiroLadrilhoQuadrado != 1 && (InformacoesUltimoVerde[1] == 2 || ObjetivoGiroLadrilhoQuadrado != 73)) || (bc.Compass()-ObjetivoGirar < 0 && bc.Compass()-ObjetivoGirar > -10 && Lado == -1 && LadrilhoQuadrado && ObjetivoGiroLadrilhoQuadrado != 1 && (InformacoesUltimoVerde[1] == 1 || ObjetivoGiroLadrilhoQuadrado != -73))) {
				ContinuarDoisPretos = false;
				LadrilhoQuadrado = false;
				break;
			}
		} else if(bc.ReturnColor(0) == "PRETO" || bc.ReturnColor(1) == "PRETO") {
			ContinuarDoisPretos = false;
			break;
		} else {
			bc.Move(1000*Lado, 1000*-Lado);
		}
	}
	if((Lado == -1 && ObjetivoGiroLadrilhoQuadrado != -17) || ObjetivoGiroLadrilhoQuadrado == 17) {
		VarTempoEsq = bc.Timer();
	} else {
		VarTempoDir = bc.Timer();
	}
}

void DoisPretos() {
	bc.Move(0, 0);
	bc.Wait(48);
	if((bc.ReturnBlue(0) > bc.ReturnRed(0)+7 && bc.ReturnBlue(0) > bc.ReturnGreen(0) && bc.ReturnRed(0) < 90) || (bc.ReturnBlue(1) > bc.ReturnRed(1)+7 && bc.ReturnBlue(1) > bc.ReturnGreen(1) && bc.ReturnRed(1) < 90)) {
		Prata();
		return;
	} else if(bc.ReturnGreen(0) > bc.ReturnBlue(0)+45 || bc.ReturnColor(0) == "VERDE" || bc.ReturnGreen(1) > bc.ReturnBlue(1)+45 || bc.ReturnColor(1) == "VERDE") {
		Verde();
		return;
	} else if(bc.Inclination() < 341 && bc.Inclination() > 330 && (bc.Lightness(0) > 50 || bc.Lightness(1) > 50)) {
		VarTempo = bc.Timer();
		while(bc.Timer()-VarTempo < 800 && bc.ReturnColor(0) == "BRANCO" && bc.ReturnColor(1) == "BRANCO") {
			bc.Move(150, 150);
		}
		return;
	} else if((RedzoneCompleta || VitimasVivasResgatadas >= 3) && (bc.ReturnRed(0) > bc.ReturnGreen(0) + 15 || bc.ReturnRed(1) > bc.ReturnGreen(1) + 15 || bc.ReturnColor(0) == "VERMELHO" || bc.ReturnColor(1) == "VERMELHO")) {
		Alinhar(true);
		MoverTempo(300, 300, 350, true);
		while(true) {
			bc.Wait(1000);
		}
	} else if(bc.Timer()-TempoUltimoDoisPretos < 350) {
		MoverTempo(-130, -130, 200, true);
		ContagemDir = 0;
		ContagemEsq = 0;
		return;
	} else {
		if(!LadrilhoQuadrado) {
			LadrilhoQuadrado = (Angulo(0)%360 == InformacoesUltimoVerde[2]%360 && bc.Timer()-InformacoesUltimoVerde[3] < 480 && bc.Timer()-InformacoesUltimoVerde[3] > 220);
		}
		int[] ValoresLuz = {0, 0};
		int[] ValoresTempo = {0, 0, 0};
		VarTempo = bc.Timer();
		while(bc.Timer()-VarTempo < 430) {
			if(bc.Lightness(0) > 40 && ValoresLuz[0] == 0) {
				ValoresLuz[0] = bc.Timer()-VarTempo;
			}
			if(bc.Lightness(1) > 40 && ValoresLuz[1] == 0) {
				ValoresLuz[1] = bc.Timer()-VarTempo;
			}
			bc.Move(150, 150);
		}
		ContinuarDoisPretos = true;
		if(ValoresLuz[0] > 150 || ValoresLuz[1] > 150 || ValoresLuz[0] == 0 || ValoresLuz[1] == 0) {
			ValoresTempo = new int[] {300, 3000, 5400};
		} else {
			ValoresTempo = new int[] {0, 2700, 5100};
		}
		if(ContagemEsq > ContagemDir) {
			if(ValoresTempo[0] != 0) {
				GiroPreto(1, ValoresTempo[0], 1);
			}
			if(ContinuarDoisPretos) {
				GiroPreto(-1, ValoresTempo[1], -73);
				if(ContinuarDoisPretos) {
					GiroPreto(1, ValoresTempo[2], 1);
					if(ContinuarDoisPretos) {
						LadrilhoQuadrado = true;
						if(InformacoesUltimoVerde[1] == 1) {
							GiroPreto(-1, 20000, -163);
						} else {
							GiroPreto(-1, 20000, -17);
						}
					}
				}
			}
		} else {
			if(ValoresTempo[0] != 0) {
				GiroPreto(-1, ValoresTempo[0], 1);
			}
			if(ContinuarDoisPretos) {
				GiroPreto(1, ValoresTempo[1], 73);
				if(ContinuarDoisPretos) {
					GiroPreto(-1, ValoresTempo[2], 1);
					if(ContinuarDoisPretos) {
						LadrilhoQuadrado = true;
						if(InformacoesUltimoVerde[1] == 2) {
							GiroPreto(1, 20000, 163);
						} else {
							GiroPreto(1, 20000, 17);
						}
					}
				}	
			}
		}
		if(bc.ReturnColor(0) == "PRETO") {
			while(bc.ReturnColor(0) == "PRETO" && bc.ReturnColor(1) == "BRANCO") {
				bc.Move(1000, -1000);
			}
			if(bc.ReturnColor(1) != "PRETO") {
				bc.Wait(100);
			} else {
				bc.Move(0, 0);
			}
		} else if(bc.ReturnColor(1) == "PRETO") {
			while(bc.ReturnColor(1) == "PRETO" && bc.ReturnColor(0) == "BRANCO") {
				bc.Move(-1000, 1000);
			}
			if(bc.ReturnColor(0) != "PRETO") {
				bc.Wait(100);
			} else {
				bc.Move(0, 0);
			}
		}
		TempoUltimoDoisPretos = bc.Timer();
		Luz = new float[] {bc.Lightness(0), bc.Lightness(1)};
	}
}

void Verde() {
	if(bc.ReturnGreen(0) > bc.ReturnBlue(0)+20 || bc.ReturnColor(0) == "VERDE") {
		bc.Wait(10);
		if(bc.ReturnGreen(0) > bc.ReturnBlue(0)+20 || bc.ReturnColor(0) == "VERDE") {
			GiroVerde(10, -1000, 0, 1);
		}
	} else if(bc.ReturnGreen(1) > bc.ReturnBlue(1)+20 || bc.ReturnColor(1) == "VERDE") {
		bc.Wait(10);
		if(bc.ReturnGreen(1) > bc.ReturnBlue(1)+20 || bc.ReturnColor(1) == "VERDE") {
			GiroVerde(-10, 1000, 1, 0);
		}
	}
}

void Tendencioso(float ForcaDireita, float ForcaEsquerda) {
	bc.Move(ForcaEsquerda, ForcaDireita);
	if(VarTempoEsq > VarTempoDir && Diferenca(0) < 0 && Diferenca(0) > -32 && bc.Timer()-VarTempoDir > 350 && bc.Timer()-VarTempoEsq > 350) {
		VarTempoDir = bc.Timer();
		VarTempoTendencioso = bc.Timer();
		ValorTendencioso = -4f*Math.Abs(Diferenca(0))-89;
	} else if(VarTempoEsq < VarTempoDir && Diferenca(0) > 0 && Diferenca(0) < 32 && bc.Timer()-VarTempoDir > 350 && bc.Timer()-VarTempoEsq > 350) {
		VarTempoEsq = bc.Timer();
		VarTempoTendencioso = bc.Timer();
		ValorTendencioso = -4f*Math.Abs(Diferenca(0))-89;
	} else if(bc.Timer()-VarTempoTendencioso > 1000) {
		ValorTendencioso = -2.2f*Math.Abs(Diferenca(0))-89;
	}
}

void Preto() {
	if((bc.ReturnColor(0) == "PRETO" || Luz[0]-bc.Lightness(0) > (-0.3*bc.Lightness(0))+50 || bc.Lightness(0) < 55) && bc.ReturnGreen(0) < bc.ReturnBlue(0)+20 && bc.ReturnColor(0) != "VERDE") {
		if((bc.ReturnColor(1) == "PRETO" || Luz[1]-bc.Lightness(1) > (-0.3*bc.Lightness(1))+50 || bc.Lightness(1) < 55) && bc.ReturnGreen(1) < bc.ReturnBlue(1)+20 && bc.ReturnColor(1) != "VERDE") {
			DoisPretos();
		} else {
			bc.Move(1000, -1000);
			bc.Wait(16);
			ValorTendencioso = -2.2f*Math.Abs(Diferenca(0))-89;
			VarTempoDir = bc.Timer();
			ContagemParado = 0;
			ContagemDir += 1;
			if(bc.ReturnColor(1) == "PRETO" || bc.Lightness(1) < 55) {
				DoisPretos();
			}		
		}
	} else if((bc.ReturnColor(1) == "PRETO" || Luz[1]-bc.Lightness(1) > (-0.3*bc.Lightness(1))+50 || bc.Lightness(1) < 55) && bc.ReturnGreen(1) < bc.ReturnBlue(1)+20 && bc.ReturnColor(1) != "VERDE") {
		if((bc.ReturnColor(0) == "PRETO" || Luz[0]-bc.Lightness(0) > (-0.3*bc.Lightness(0))+50 || bc.Lightness(0) < 55) && bc.ReturnGreen(0) < bc.ReturnBlue(0)+20 && bc.ReturnColor(0) != "VERDE") {
			DoisPretos();
		} else {		
			bc.Move(-1000, 1000);
			bc.Wait(16);
			ValorTendencioso = -2.2f*Math.Abs(Diferenca(0))-89;
			VarTempoEsq = bc.Timer();
			ContagemParado = 0;
			ContagemEsq += 1;
			if(bc.ReturnColor(0) == "PRETO" || bc.Lightness(0) < 55) {
				DoisPretos();
			}
		}
	} else {
		if(VarTempoEsq > VarTempoDir && Math.Abs(Diferenca(0)) > 1) {
			Tendencioso(300, ValorTendencioso);
		} else if(VarTempoDir > VarTempoEsq && Math.Abs(Diferenca(0)) > 1) {
			Tendencioso(ValorTendencioso, 300);
		} else {
			bc.Move(Velocidade, Velocidade);
		}
		Luz = new float[] {bc.Lightness(0), bc.Lightness(1)};
		bc.Wait(1);
	}
	if(bc.Timer()-VarTempoDir > 60 && bc.Timer()-VarTempoEsq > 60) {
		ContagemDir = 0;
		ContagemEsq = 0;
	} else if(ContagemEsq > 50 && ContagemDir > 50) {
		DoisPretos();
		ContagemDir = 0;
		ContagemEsq = 0;
	}
}

void Desviar(int Tempo) {
	VarTempo = bc.Timer();
	while(bc.Timer()-VarTempo < Tempo) {
		if(bc.Lightness(1) < 40) {
			DesvioVerdadeiro = false;
			break;
		} else {
			bc.Move(180, 180);
		}
	}
}

void Desvio() {
	if(bc.Distance(0) < 70) {
		while(bc.Distance(0) < 75) {
			if(bc.Distance(0) < 32) {
				DesvioVerdadeiro = true;
				break;
			} else {
				Preto();
				Verde();
				Prata();
				Parado();
				KitResgate();
			}
		}
		if(DesvioVerdadeiro) {
			Girar(45, 1);
			Desviar(730);
			if(DesvioVerdadeiro) {
				Girar(-45, 1);
				bc.MoveFrontalRotations(180, 16.5f);
				Girar(-57, 1);
				Desviar(480);
				if(DesvioVerdadeiro) {
					Girar(-45, 1);
					bc.MoveFrontalRotations(180, 10);
					Girar(-45, 1);
					Desviar(int.MaxValue);
				}
			}
			bc.MoveFrontalRotations(180, 14);
			int LimiteGiro = Objetivo(90, 1);
			while(Math.Abs(bc.Compass()-LimiteGiro) > 2) {
				if(bc.ReturnColor(0) == "PRETO") {
					MoverTempo(-1000, 1000, 100, false);
					break;
				} else if(bc.ReturnColor(1) == "PRETO") {
					MoverTempo(1000, -1000, 100, false);
					break;
				} else {
					bc.Move(1000, -1000);
				}
			}
			if(Math.Abs(bc.Compass()-LimiteGiro) < 2) {
				Girar(-45, 1);
			}
			if(Math.Abs(Diferenca(0)) < 25) {
				bc.Wait(250);
				VarTempo = bc.Timer();
				while(!bc.Touch(0) && bc.Timer()-VarTempo < 450) {
					bc.Move(-180, -180);
				}
				MoverTempo(-180, -180, 80, false);
			}
		}
		DesvioVerdadeiro = false;
		Luz = new float[] {bc.Lightness(0), bc.Lightness(1)};
		Verde();
	}
}

void Alinhar(bool VerAngulo) {
	if(Math.Abs(Diferenca(0)) <= 4 && VerAngulo) {
		return;
	} else {
		float VarAngulo = Angulo(0);
		if(bc.Compass() >= 315) {
			while(bc.Compass() < 359 && bc.ReturnColor(0) == "BRANCO" && bc.ReturnColor(1) == "BRANCO") {
				bc.Move(1000, -1000);
			}
		} else {
			if(bc.Compass() > VarAngulo) {
				if(VarAngulo == 0) {
					VarAngulo = 1;
				}
				while(bc.Compass() > VarAngulo && bc.ReturnColor(0) == "BRANCO" && bc.ReturnColor(1) == "BRANCO") {
					bc.Move(-1000, 1000);
				}
			} else {
				if(VarAngulo == 0) {
					VarAngulo = 359;
				}
				while(bc.Compass() < VarAngulo && bc.ReturnColor(0) == "BRANCO" && bc.ReturnColor(1) == "BRANCO") {
					bc.Move(1000, -1000);
				}
			}
		}
		bc.Move(0, 0);
	}
}

void BaixaGarra(bool AbrirGarra) {
	while(bc.AngleActuator() > 2) {
		bc.ActuatorDown(1);
	}
	bc.ActuatorDown(40);
	if(AbrirGarra) {
		bc.OpenActuator();
	}
}

void LevantaGarra() {
	bc.CloseActuator();
	bc.TurnActuatorUp(100);
	bc.ActuatorUp(30);
	while(bc.AngleActuator() < 87) {
		bc.ActuatorUp(1);
	}
	bc.ActuatorUp(50);
}

void Rampa() {
	if(bc.Inclination() < 341 && bc.Inclination() > 330) {
		int TempoInicial = bc.Timer();
		while(bc.Timer()-TempoInicial < 100) {
			Preto();
			Verde();
			Prata();
			Parado();
			Desvio();
			KitResgate();
		}
		if(bc.Inclination() < 341 && bc.Inclination() > 330) {
			TempoInicial = bc.Timer();
			Velocidade = 130;
			while(true) {
				Preto();
				Verde();
				Prata();
				Parado();
				Desvio();
				KitResgate();
				if(bc.Inclination() > 350 || bc.Inclination() < 30) {
					TempoInicial = bc.Timer();
					while(bc.Timer()-TempoInicial < 1200) {
						Preto();
						Verde();
						Prata();
						Parado();
						Desvio();
						KitResgate();
						if(bc.Inclination() > 8 && bc.Inclination() < 30) {
							MoverTempo(150, 150, 300, false);
							break;
						}
					}
					if(bc.Inclination() > 8 && bc.Inclination() < 30) {
						Alinhar(true);
					}
					break;
				} else if(bc.Inclination() > 344 && bc.Timer()-TempoInicial < 1950) {
					bc.Move(0, 0);
					bc.Wait(2500);
					Alinhar(true);
					break;
				}
			}
			Luz = new float[] {bc.Lightness(0), bc.Lightness(1)};
			Velocidade = 130;
		}
	}
}

void SeguidorRedzone() {
	while(bc.ReturnGreen(0) < bc.ReturnRed(0)+10 && bc.ReturnGreen(1) < bc.ReturnRed(1)+10 && bc.ReturnColor(0) != "VERDE" && bc.ReturnColor(1) != "VERDE") {
		bc.Move(150, 150);
	}
	while(bc.ReturnGreen(0) > bc.ReturnRed(0)+10 || bc.ReturnGreen(1) > bc.ReturnRed(1)+10 || bc.ReturnColor(0) == "VERDE" || bc.ReturnColor(1) == "VERDE") {
		bc.Move(100, 100);
	}
	RedzoneCompleta = true;
	bc.ResetTimer();
	bc.Wait(100);
	VarTempo = 0;
	Velocidade = 130;
	VarTempoEsq = 0;
	VarTempoDir = 0;
	ContagemParado = 0;
	TempoUltimoDoisPretos = 0;
	InformacoesUltimoVerde = new [] {0, 0, 0, 0, 0};
	Luz = new float[] {bc.Lightness(0), bc.Lightness(1)};
	LadrilhoQuadrado = false;
	while(true) {
		Preto();
		Verde();
		Rampa();
		Parado();
		Desvio();
		if(bc.ReturnRed(0) > bc.ReturnGreen(0) + 15 || bc.ReturnRed(1) > bc.ReturnGreen(1) + 15 || bc.ReturnColor(0) == "VERMELHO" || bc.ReturnColor(1) == "VERMELHO") {
			bc.Wait(10);
			if(bc.ReturnRed(0) > bc.ReturnGreen(0) + 15 || bc.ReturnRed(1) > bc.ReturnGreen(1) + 15 || bc.ReturnColor(0) == "VERMELHO" || bc.ReturnColor(1) == "VERMELHO") {
				Alinhar(true);
				MoverTempo(300, 300, 350, true);
				while(true) {
					bc.Wait(1000);
				}
			}
		}
	}
}

float TamanhoRedzone(int Modo) {
	if((Array.IndexOf(ValoresRedzone, Angulo(0)%360) <= 1 && Modo == 0) || (Array.IndexOf(ValoresRedzone, Angulo(0)%360) >= 2 && Modo == 1)) {
		return 4;
	} return 2.8f;
}

void AndarReto(float ForcaFrente) {
	if(Diferenca(0) > 0.2) {
		if(ForcaFrente >= 0) {
			bc.Move((float)(-20*Math.Abs(Diferenca(0))), 300);
		} else {
			bc.Move(-300, (float)(20*Math.Abs(Diferenca(0))));
		}
	} else if(Diferenca(0) < -0.2) {
		if(ForcaFrente >= 0) {
			bc.Move(300, (float)(-20*Math.Abs(Diferenca(0))));
		} else {
			bc.Move((float)(20*Math.Abs(Diferenca(0))), -300);
		}
	} else {
		bc.Move(ForcaFrente, ForcaFrente);
	}
}

void AndarCaso(float Velocidade1, float Velocidade2) {
	if(PosicaoTrianguloOriginal == 1) {
		AndarReto(Velocidade1);
	} else {
		AndarReto(Velocidade2);
	}
}

void AlinharSaida(int DistanciaUsada) {
	if(bc.Distance(0) < DistanciaUsada) {
		while(bc.Distance(0) < DistanciaUsada-3) {
			bc.Move(-300, -300);
		}	
	} else {
		while(bc.Distance(0) > DistanciaUsada+3) {
			bc.Move(300, 300);
		}
	}
}

int VerificacaoFita() {
	if(bc.ReturnGreen(0) > bc.ReturnRed(0)+20 || bc.ReturnGreen(1) > bc.ReturnRed(1)+20 || bc.ReturnColor(0) == "VERDE" || bc.ReturnColor(1) == "VERDE") {
		bc.Wait(25);
		if(bc.ReturnGreen(0) > bc.ReturnRed(0)+20 || bc.ReturnGreen(1) > bc.ReturnRed(1)+20 || bc.ReturnColor(0) == "VERDE" || bc.ReturnColor(1) == "VERDE") {
			return 1;
		}
	} else if((bc.ReturnBlue(0) > bc.ReturnRed(0)+7 && bc.ReturnBlue(0) > bc.ReturnGreen(0) && bc.ReturnRed(0) < 90) || (bc.ReturnBlue(1) > bc.ReturnRed(1)+7 && bc.ReturnBlue(1) > bc.ReturnGreen(1) && bc.ReturnRed(1) < 90)) {
		MoverTempo(100, 100, 50, true);
		if((bc.ReturnBlue(0) > bc.ReturnRed(0)+7 && bc.ReturnBlue(0) > bc.ReturnGreen(0) && bc.ReturnRed(0) < 90) || (bc.ReturnBlue(1) > bc.ReturnRed(1)+7 && bc.ReturnBlue(1) > bc.ReturnGreen(1) && bc.ReturnRed(1) < 90)) {
			return 2;
		}
	}
	return 0;
}

void ProcurarSaida() {
	Girar(-90, 1);
	bool IgnorarVerificaçãoInicial = false;
	while(true) {
		if(VerificacaoFita() == 1) {
			MoverTempo(-150, -150, 790, true);
			break;
		} else if(VerificacaoFita() == 2) {
			MoverTempo(-150, -150, 790, true);
			IgnorarVerificaçãoInicial = true;
			break;
		} else if(bc.Distance(0) < 55) {
			break;
		} else {
			bc.Move(250, 250);
		}
	}
	Girar(-45, 1);
	if(IgnorarVerificaçãoInicial) {
		MoverTempo(250, 250, 750, true);
	}
	bc.ResetTimer();
	VarTempo = 0;
	int VerificacaoSaida = 0;
	int TempoInicial = 0;
	while(true) {
		if(bc.Distance(1) > 55 && (bc.Timer()-VerificacaoSaida > 1000 || (!IgnorarVerificaçãoInicial && bc.Timer()-TempoInicial < 1800))) {
			if(IgnorarVerificaçãoInicial && bc.Timer()-VarTempo > 1800) {
				IgnorarVerificaçãoInicial = false;
			} else {
				if(bc.Distance(0) < 80) {
					AlinharSaida(40);
				} else if(bc.Distance(0) < 180) {
					AlinharSaida(139);
				} else if(bc.Distance(0) < 280) {
					AlinharSaida(239);
				} else if(bc.Distance(0) < 380) {
					AlinharSaida(339);
				} else {
					bc.Move(300, 300);
					if(bc.Timer()-VarTempo < 200) {
						bc.Wait(150);
					} else {
						bc.Wait(350);
					}
				}
				Girar(90, 0);
				VerificacaoSaida = bc.Timer();
				while(bc.Timer()-VerificacaoSaida < 2200) {
					if(VerificacaoFita() == 1) {
						SeguidorRedzone();
					} else if(VerificacaoFita() == 2) {
						break;
					}
					bc.Move(150, 150);
				}
				MoverTempo(-150, -150, 450, true);
				Girar(-90, 0);
				VerificacaoSaida = bc.Timer();
			}
		} else {
			AndarReto(250);
			if(VerificacaoFita() == 1) {
				SeguidorRedzone();
			} else if(VerificacaoFita() == 2) {
				MoverTempo(-300, -300, 300, true);
				Girar(-90, 0);
				IgnorarVerificaçãoInicial = true;
				VerificacaoSaida = bc.Timer();
			} else if(bc.Distance(0) < 43) {
				Girar(-90, 0);
			}
		}
	}
}

void VoltarProcurarBolinhas() {
	if(ResgatouBolinha) {
		bc.Move(0, 0);
		LevantaGarra();
		MoverTempo(-50, -50, 200, true);
		if(VitimasVivasResgatadas >= 3) {
			ProcurarSaida();
		} else if(PosicaoTrianguloOriginal == 1) {
			Girar(135, 1);
		} else {
			Girar(45, 1);
		}
		PosicaoTriangulo = 3;
	} else if(IrBuscarBolinha) {
		bc.ResetTimer();
		while(bc.Timer() < (TempoFrente/2)+140-TempoTras || bc.Distance(0) < 82*TamanhoRedzone(0)) {
			bc.Move(-300, -300);
		}
		Girar(-90, 0);
		if(PosicaoTrianguloOriginal == 1) {
			MoverTempo(300, 300, 280, false);
		} else {
			MoverTempo(-300, -300, 200, false);
		}
	} else {
		IrBuscarBolinha = true;
		PosicaoTriangulo = 3;
	}
	UltrassonicoBolinha = 0;
	if(PosicaoTriangulo != PosicaoTrianguloOriginal && PosicaoTrianguloOriginal != 3) {
		PosicaoTriangulo = PosicaoTrianguloOriginal;
		if(PosicaoTriangulo == 1) {
			while(bc.Distance(0) > 35) {
				Ultrassonico = bc.Distance(1);
				AndarReto(250);
				bc.Wait(60);
				if(((bc.Distance(1) < Ultrassonico-7 && Ultrassonico < 400) || bc.Distance(1) < 70*TamanhoRedzone(1)) && (bc.Distance(0) <= MaiorDistanciaPercorrida || MaiorDistanciaPercorrida == 0 || VitimasVivasResgatadas >= 2)) {
					PosicaoTriangulo = 3;
					UltrassonicoBolinha = bc.Distance(1);
					break;
				} else if(VerificacaoFita() == 1 || VerificacaoFita() == 2) {
					MoverTempo(-300, -300, 350, true);
					break;
				}
			}
			if(UltrassonicoBolinha == 0) {
				Girar(90, 0);
				MoverTempo(300, 300, 300, false);
			}
		} else if(PosicaoTriangulo == 2) {
			while(bc.Distance(0) < 85*TamanhoRedzone(0)) {
				Ultrassonico = bc.Distance(1);
				AndarReto(-300);
				bc.Wait(50);
				if(((bc.Distance(1) < Ultrassonico-7 && Ultrassonico < 400) || bc.Distance(1) < 70*TamanhoRedzone(1)) && (bc.Distance(0) > MaiorDistanciaPercorrida || VitimasVivasResgatadas >= 2)) {
					PosicaoTriangulo = 3;
					UltrassonicoBolinha = bc.Distance(1);
					break;
				}
			}
			if(UltrassonicoBolinha == 0) {
				Girar(-90, 0);
				MoverTempo(-300, -300, 200, false);
			}
		}
	}
}

void BuscarBolinha() {
	if(PosicaoTriangulo == 3) {
		MaiorDistanciaPercorrida = UltrassonicoFrente;
	}
	Girar(90, 0);
	if(UltrassonicoBolinha < 45 && VitimasVivasResgatadas >= 2) {
		TempoTras = (int)(((-13*UltrassonicoBolinha)+710)/2);
		MoverTempo(-150, -150, TempoTras*2, true);
	}
	bc.ResetTimer();
	while(UltrassonicoBolinha > 50 && bc.Timer() < (10*UltrassonicoBolinha)-420) {
		AndarReto(300);
	}
	TempoFrente = bc.Timer()*2;
	if(VitimasVivasResgatadas < 2) {
		bc.ResetTimer();
		while(bc.Distance(0) > 40 && VerificacaoFita() == 0 && bc.Timer() < 2500) {
			AndarReto(150);
			if((bc.ReturnRed(2) < 50 || bc.ReturnRed(2) >= 51) && (bc.ReturnGreen(2) < 50 || bc.ReturnGreen(2) >= 51) && (bc.ReturnBlue(2) < 50 || bc.ReturnBlue(2) >= 51)) {
				break;
			}
		}
		bc.MoveFrontalRotations(150, 0.7f);
		TempoFrente = bc.Timer()+TempoFrente;
	} else {
		bc.Move(0, 0);
	}
	if(bc.Lightness(2) > 30 || VitimasVivasResgatadas >= 2) {
		if(VitimasVivasResgatadas < 2) {
			bc.ResetTimer();
			MoverTempo(-300, -300, 350, true);
			TempoTras = bc.Timer();
			if(PosicaoTrianguloOriginal == 1) {
				MaiorDistanciaPercorrida += 15;
			} else {
				MaiorDistanciaPercorrida -= 15;
			}
		}
		BaixaGarra(false);
		bc.ResetTimer();
		while(!bc.HasVictim() && bc.Distance(0) > 48 && VerificacaoFita() == 0 && bc.Timer() < 3750) {
			bc.Move(100, 100);
		}
		bc.Move(100, 100);
		LevantaGarra();
		TempoFrente = (int)((bc.Timer()*0.65))+TempoFrente;
		Alinhar(true);
		if(bc.HasVictim()) {
			if(PosicaoTriangulo == 3) {
				bc.ResetTimer();
				while(bc.Timer() < (TempoFrente/2)+140-TempoTras || bc.Distance(0) < 82*TamanhoRedzone(0)) {
					AndarReto(-300);
				}
				TempoTras = 0;
				bc.Move(0, 0);
			} else {
				bc.ResetTimer();
				while(true) {
					if(VerificacaoFita() == 1 || VerificacaoFita() == 2) {
						MoverTempo(-300, -300, 700, true);
						break;
					} else if(bc.Distance(0) < 75) {
						break;
					} else {
						AndarReto(250);
					}
				}
				if(bc.Distance(0) < 70) {
					while(bc.Distance(0) < 73) {
						AndarReto(-150);
					}
				}
			}
			if(PosicaoTriangulo == 1 || PosicaoTrianguloOriginal == 1) {
				Girar(90, 0);
			} else {
				Girar(-90, 0);
			}
			while(bc.Lightness(2) > 25) {
				AndarReto(300);
			}
			if(bc.Distance(0) > 90 && PosicaoTriangulo == 3) {
				float DisanciaVista = bc.Distance(0);
				if(PosicaoTrianguloOriginal == 1) {
					Girar(-45, 0);
					bc.MoveFrontalRotations(150, (0.5f*DisanciaVista)-40);
					Girar(90, 1);
				} else {
					Girar(45, 0);
					bc.MoveFrontalRotations(150, (0.5f*DisanciaVista)-40);
					Girar(-90, 1);
				}
			} else if(PosicaoTrianguloOriginal == 1 && PosicaoTriangulo == 3 || PosicaoTriangulo == 2) {
				Girar(45, 0);
			} else {
				Girar(-45, 0);
			}
			MoverTempo(150, 150, 300, true);
			BaixaGarra(false);
			bc.TurnActuatorDown(100);
			while(bc.HasVictim()) {
				MoverTempo(300, 300, 100, false);
				MoverTempo(-300, -300, 100, false);
			}
			VitimasVivasResgatadas += 1;
			ResgatouBolinha = true;
		} else {
			ResgatouBolinha = false;
		}
	} else {
		ResgatouBolinha = false;
	}
	VoltarProcurarBolinhas();	
}

void ProcurarBolinha() {
	Ultrassonico = 0;
	while(true) {
		while(true) {
			Ultrassonico = bc.Distance(1);
			AndarCaso(300, -300);
			bc.Wait(25);
			if(((bc.Distance(1) < Ultrassonico-7 && Ultrassonico < 400) || bc.Distance(1) < 70*TamanhoRedzone(1) || UltrassonicoBolinha != 0) && (bc.Distance(0) > MaiorDistanciaPercorrida || PosicaoTriangulo != 3 || VitimasVivasResgatadas >= 2 || (bc.Distance(0) <= MaiorDistanciaPercorrida && PosicaoTrianguloOriginal == 1))) {
				UltrassonicoBolinha = bc.Distance(1);
				UltrassonicoFrente = bc.Distance(0);
				break;
			} else if(PosicaoTriangulo == 3 && bc.Distance(0) > 83*TamanhoRedzone(0) && PosicaoTrianguloOriginal != 1) {
				Girar(-90, 0);
				MoverTempo(-300, -300, 300, false);
				PosicaoTriangulo = 2;
			}
		}
		bc.ResetTimer();
		while(true) {
			AndarCaso(150, -150);
			if(bc.Distance(1) < UltrassonicoBolinha-10) {
				UltrassonicoBolinha = bc.Distance(1);
				bc.ResetTimer();
			} else if(bc.Distance(1) > UltrassonicoBolinha+7 || bc.Distance(0) < 30 || bc.Touch(0) || bc.Timer() > 1800) {
				break;
			}
		}
		int Timer = bc.Timer();
		if(bc.Timer() <= 1800) {
			bc.ResetTimer();
			while(bc.Timer() < 115) {
				AndarCaso(-150, 150);
			}
			while((bc.Distance(1) > UltrassonicoBolinha+7 || bc.Distance(1) < UltrassonicoBolinha-7) && bc.Timer() < 1800) {
				AndarCaso(-150, 150);
			}
			UltrassonicoBolinha = bc.Distance(1);
			if(bc.Timer() <= 1800) {
				if(Timer > 450 || bc.Distance(0) < 30) {
					bc.ResetTimer();
					while(bc.Distance(1) < UltrassonicoBolinha+7 && bc.Timer() < 2400) {
						AndarCaso(-150, 150);
					}
					if(PosicaoTrianguloOriginal == 1) {
						bc.MoveFrontalRotations(150, 3f);
					} else {
						bc.MoveFrontalRotations(-150, 7.4f);
					}
				} else if(PosicaoTrianguloOriginal == 1) {
					bc.MoveFrontalRotations(-150, 5.8f);
				} else {
					bc.MoveFrontalRotations(150, 1.6f);
				}
				BuscarBolinha();	
			} else {
				IrBuscarBolinha = false;
				ResgatouBolinha = false;
				VoltarProcurarBolinhas();
			}
		} else {
			IrBuscarBolinha = false;
			ResgatouBolinha = false;
			VoltarProcurarBolinhas();
		}
	}
}

void Redzone() {
	VarTempo = bc.Timer();
	while(bc.Timer()-VarTempo < 800) {
		Ultrassonico = bc.Distance(1);
		MoverTempo(150, 150, 16, false);
		if(bc.Distance(1)-Ultrassonico > 0.5 && bc.Distance(1)-Ultrassonico < 2) {
			ProbabilidadeTriangulo += 1;
		}
	}
	float UltraLateral = 0;
	if(bc.Distance(0) <= 320) {
		ValoresRedzone = new[] {Objetivo(92, 0), Objetivo(-92, 0), Angulo(0)%360, Objetivo(182, 0)};
	} else if(bc.Distance(0) < 400) {
		ValoresRedzone = new[] {Angulo(0)%360, Objetivo(182, 0), Objetivo(92, 0), Objetivo(272, 0)};
	} else {
		UltraLateral = bc.Distance(1);
		MoverTempo(150, 150, 300, false);
		Girar(-90, 0);
		if(bc.Distance(0)+UltraLateral > 320 || (bc.Distance(0)+UltraLateral > 270 && ProbabilidadeTriangulo >= 30)) {
			ValoresRedzone = new[] {Angulo(0)%360, Objetivo(182, 0), Objetivo(92, 0), Objetivo(272, 0)};
		} else {
			ValoresRedzone = new[] {Objetivo(92, 0), Objetivo(-92, 0), Angulo(0)%360, Objetivo(182, 0)};
		}
	}
	if(ProbabilidadeTriangulo >= 20) {
		PosicaoTriangulo = 1;
	}
	if(UltraLateral == 0) {
		MoverTempo(150, 150, 300, false);
	}
	if(PosicaoTriangulo != 1) {
		if(bc.Distance(1) < 75*TamanhoRedzone(1) || UltraLateral != 0 || bc.Distance(1) > 380) {
			if(UltraLateral == 0 || (bc.Distance(1) > 380 && UltraLateral == 0)) {
				Girar(-90, 0);
			}
			while(true) {
				AndarReto(300);
				if(bc.Lightness(2) < 25) {
					PosicaoTriangulo = 3;
					break;
				} else if(bc.Distance(0) < 45) {
					break;
				} else if(VerificacaoFita() == 1) {
					MoverTempo(-300, -300, 350, true);
					break;
				}
			}
			if(PosicaoTriangulo == 0) {
				Girar(90, 0);
			}
		}
		if(PosicaoTriangulo == 0) {
			while(true) {
				AndarReto(300);
				if(bc.Lightness(2) < 25) {
					PosicaoTriangulo = 3;
					break;
				} else if(bc.Distance(0) < 45) {
					PosicaoTriangulo = 2;
					break;
				} else if(VerificacaoFita() == 1) {
					MoverTempo(-300, -300, 400, true);
					PosicaoTriangulo = 2;
					break;
				}
			}	
		}
	} else if(PosicaoTriangulo == 1 && UltraLateral != 0) {
		Girar(90, 0);
	}
	PosicaoTrianguloOriginal = PosicaoTriangulo;
	if(PosicaoTriangulo == 1 || PosicaoTriangulo == 2) {
		Girar(90, 0);
	} else {
		Girar(-45, 0);
	}
	while(bc.Lightness(2) > 25) {
		AndarReto(300);
	}
	if(PosicaoTriangulo == 1) {
		Girar(45, 0);
	} else if(PosicaoTriangulo == 2) {
		Girar(-45, 0);
	}
	MoverTempo(150, 150, 300, true);
	BaixaGarra(false);
	bc.TurnActuatorDown(100);
	bc.ResetTimer();
	while(bc.HasRescueKit() && bc.Timer() < 300) {
		MoverTempo(300, 300, 100, false);
		MoverTempo(-300, -300, 100, false);
	}
	ResgatouBolinha = true;
	VoltarProcurarBolinhas();
	ProcurarBolinha();
}

void Prata() {
	if((bc.ReturnBlue(0) > bc.ReturnRed(0)+7 && bc.ReturnBlue(0) > bc.ReturnGreen(0) && bc.ReturnRed(0) < 90) || (bc.ReturnBlue(1) > bc.ReturnRed(1)+7 && bc.ReturnBlue(1) > bc.ReturnGreen(1) && bc.ReturnRed(1) < 90)) {
		bc.Wait(25);
		if((bc.ReturnBlue(0) > bc.ReturnRed(0)+7 && bc.ReturnBlue(0) > bc.ReturnGreen(0) && bc.ReturnRed(0) < 90) || (bc.ReturnBlue(1) > bc.ReturnRed(1)+7 && bc.ReturnBlue(1) > bc.ReturnGreen(1) && bc.ReturnRed(1) < 90)) {
			int PrimeiroSensorVisto = 0;
			if(bc.ReturnBlue(0) > bc.ReturnRed(0)+7 && bc.ReturnBlue(0) > bc.ReturnGreen(0) && bc.ReturnRed(0) < 90) {
				PrimeiroSensorVisto = 0;
			} else {
				PrimeiroSensorVisto = 1;
			}
			VarTempo = bc.Timer();
			while(bc.Timer()-VarTempo < 150 && bc.ReturnColor(0) == "BRANCO" && bc.ReturnColor(1) == "BRANCO") {
				bc.Move(150, 150);
			}
			if(PrimeiroSensorVisto == 0 && Math.Abs(Diferenca(1)) < 15 && Angulo(0) != Angulo(1)) {
				MoverTempo(150, 150, 350, false);
				bc.MoveFrontalAngles(1000, 25);
			} else if(PrimeiroSensorVisto == 1 && Math.Abs(Diferenca(1)) < 15 && Angulo(0) != Angulo(1)) {
				MoverTempo(150, 150, 350, false);
				bc.MoveFrontalAngles(1000, -25);
			}
			Alinhar(true);
			VarTempo = bc.Timer();
			while(bc.Timer()-VarTempo < 865 && bc.ReturnColor(0) == "BRANCO" && bc.ReturnColor(1) == "BRANCO") {
				bc.Move(Velocidade, Velocidade);
				Verde();
				Desvio();
				if((bc.Distance(1) < 50 && bc.Distance(1) > 20) || (bc.Distance(1) > 120 && bc.Distance(1) < 150)) {
					Redzone();
				}
			}
		}
	}
}

void KitResgate() {
	if(bc.ReturnColor(2) == "CIANO") {
		MoverTempo(-300, -300, 350, true);
		BaixaGarra(true);
		VarTempo = bc.Timer();
		while(!bc.HasRescueKit() && bc.Timer()-VarTempo < 1000) {
			bc.Move(150, 150);
		}
		MoverTempo(150, 150, 450, false);
		LevantaGarra();
		int TempoMovimento = bc.Timer()-VarTempo;
		MoverTempo(-150, -150, TempoMovimento-550, false);
	}
}

void Parado() {
	if(bc.RobotSpeed() < 0.1) {
		ContagemParado += 1;
	} else {
		ContagemParado = 0;
	}
	if(ContagemParado > 25) {
		MoverTempo(1000, -1000, 25, true);
		ContagemParado = 0;
	}
}

void Main() {
	bc.ActuatorSpeed(150);
	bc.ResetTimer();
	LevantaGarra();
	if(VitimasVivasResgatadas != 0) {
		bc.TurnLedOn(255, 165, 0);
		bc.Wait(2000);
		bc.TurnLedOff();
	}
	while(true) {
		Preto();
		Verde();
		Prata();
		Rampa();
		Parado();
		Desvio();
		KitResgate();
	}
}