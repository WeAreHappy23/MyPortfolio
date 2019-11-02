// Fill out your copyright notice in the Description page of Project Settings.

#include "NavigationPoint.h"
#include "Components/BoxComponent.h"

// プレーヤーがナビゲーションアクターに来たのかを範囲オーバーラップで判断されるようにしました。
// それではイベントが発生してネイゲイションキャラクターは次のターゲットに移動することになります。

ANavigationPoint::ANavigationPoint()
{
	// ルートを生成、適用するコード
	Collision = CreateDefaultSubobject<UBoxComponent>(TEXT("Collision"));
	Collision->SetupAttachment(RootComponent);

	// コリジョンのサイズや属性設定
	Collision->SetRelativeScale3D(FVector(5.0f, 5.0f, 5.0f));
	Collision->bGenerateOverlapEvents = true;
	Collision->SetCollisionProfileName("OverlapAll");

	// タグ設定
	Collision->ComponentTags.Add(FName("DisregardForLeftHand"));
	Collision->ComponentTags.Add(FName("DisregardForRightHand"));
	Tags.Add(FName("NavigationEvent"));
}

void ANavigationPoint::BeginPlay()
{
	Super::BeginPlay();

	// オーバーラップイベントを発生させるように設定
	Collision->OnComponentBeginOverlap.AddDynamic(this, &ANavigationPoint::OnOverlap);
}

void ANavigationPoint::OnOverlap(UPrimitiveComponent * OverlappedComp, AActor * OtherActor, UPrimitiveComponent * OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult & SweepResult)
{
	if (OtherActor->ActorHasTag("Character"))
	{
		// キャラクターとオーバーラップされるとこのターゲットだけが道案内アクターのNavigationEventを使用できるようにデリゲートでバインディング
		NaviEvent.ExecuteIfBound();
	}
}
