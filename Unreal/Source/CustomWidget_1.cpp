// Fill out your copyright notice in the Description page of Project Settings.

#include "CustomWidget_1.h"
#include "Widgets/CustomButton.h"

// ウィジェットのクラスをこのクラスに登録します。 それではウィジェット上のオブジェクトと相互作用することができます。

void UCustomWidget_1::NativeConstruct()
{
	Super::NativeConstruct();

	// ウィジェットの'ExitButton','CancelButton'という名前を持ったボタンをそれぞれ'CB_1','CB_2'の変数にします。
	CB_1 = Cast<UCustomButton>(GetWidgetFromName(TEXT("ExitButton")));
	CB_2 = Cast<UCustomButton>(GetWidgetFromName(TEXT("CancelButton")));

	// ボタンが押されれば登録された関数が呼び出されます。
	CB_1->OnClicked.AddDynamic(this, &UCustomWidget_1::OnClickedCB_1);
	CB_2->OnClicked.AddDynamic(this, &UCustomWidget_1::OnClickedCB_2);
}

void UCustomWidget_1::OnClickedCB_1()
{
	UE_LOG(LogTemp, Log, TEXT("Press CB_1"));
}

void UCustomWidget_1::OnClickedCB_2()
{
	UE_LOG(LogTemp, Log, TEXT("Press CB_2"));
}