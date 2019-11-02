// Fill out your copyright notice in the Description page of Project Settings.

#include "LockKey.h"
#include "Components/StaticMeshComponent.h"
#include "UObject/ConstructorHelpers.h"
#include "Engine/StaticMesh.h"
#include "Kismet/GameplayStatics.h"							
#include "MyCharacter/MotionControllerCharacter.h"
// Sets default values
ALockKey::ALockKey()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;
	
	//  ルートを生成、適用するコード
	Key = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Key"));
	Key->SetupAttachment(RootComponent);

	// エディタ上でメッシュを探して適用
	static ConstructorHelpers::FObjectFinder<UStaticMesh>SM_Key(TEXT("StaticMesh'/Game/Assets/CharacterEquipment/Item/LockAndKey/Key/Mesh/SM_Key.SM_Key'"));
	if (SM_Key.Succeeded())
	{
		Key->SetStaticMesh(SM_Key.Object);
	}

	// 鍵の属性設定
	Key->SetEnableGravity(false);
	Key->bGenerateOverlapEvents = true;
	Key->SetCollisionProfileName("OverlapAll");
	Tags.Add("Key");
	Tags.Add(FName(TEXT("DisregardForLeftHand")));
	Tags.Add(FName(TEXT("DisregardForRightHand")));
}

// Called when the game starts or when spawned
void ALockKey::BeginPlay()
{
	Super::BeginPlay();

	// オーバーラップイベントを発生させるように設定
	Key->OnComponentBeginOverlap.AddDynamic(this, &ALockKey::OnOverlap);		
}

// オーバーラップイベント
void ALockKey::OnOverlap(UPrimitiveComponent * OverlappedComp, AActor * OtherActor, UPrimitiveComponent * OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult & SweepResult)
{
	// 鍵を獲得
	if (OtherActor->ActorHasTag("RightHand") || OtherActor->ActorHasTag("LeftHand"))
	{
		AMotionControllerCharacter* Character = Cast<AMotionControllerCharacter>(UGameplayStatics::GetPlayerCharacter(GetWorld(), 0));
		
		// 鍵が手とオーバーラップされると空間上の鍵は消え、キャラクターは鍵を獲得したものと設定される。
		if (!Character->bHasKey)
		{
			Character->bHasKey = true;
			Destroy();
		}
	}
}

