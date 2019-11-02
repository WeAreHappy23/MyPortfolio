// Fill out your copyright notice in the Description page of Project Settings.

#include "CustomWidget_1.h"
#include "Widgets/CustomButton.h"
#include "Kismet/GameplayStatics.h"
#include "MyCharacter/MotionControllerCharacter.h"

void UCustomWidget_1::NativeConstruct()
{
	Super::NativeConstruct();

	// ウィジェット内で名前でキャストする
	CB_1 = Cast<UCustomButton>(GetWidgetFromName(TEXT("ExitButton")));
	CB_2 = Cast<UCustomButton>(GetWidgetFromName(TEXT("CancelButton")));

	// キャストされた各ボタンをTouchー>Grabする場合に発生するイベントを設定する
	CB_1->OnClicked.AddDynamic(this, &UCustomWidget_1::OnClickedCB_1);
	CB_2->OnClicked.AddDynamic(this, &UCustomWidget_1::OnClickedCB_2);
}

// ゲーム終了。最初に戻る。
void UCustomWidget_1::OnClickedCB_1()
{
	Cast<AMotionControllerCharacter>(UGameplayStatics::GetPlayerCharacter(GetWorld(), 0))->MoveMainScene();
}

// ゲームを終了させるメニューを生成
void UCustomWidget_1::OnClickedCB_2()
{
	Cast<AMotionControllerCharacter>(UGameplayStatics::GetPlayerCharacter(GetWorld(), 0))->GameMenu();
}