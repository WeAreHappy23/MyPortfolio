// Fill out your copyright notice in the Description page of Project Settings.

#include "Potion.h"
#include "Components/StaticMeshComponent.h"			
#include "UObject/ConstructorHelpers.h"					
#include "MyCharacter/MotionControllerCharacter.h"
#include "Item/Table/ItemDataSingleton.h"
#include "Engine/StreamableManager.h"
#include "Engine/StaticMesh.h"
#include "Particles/ParticleSystem.h"
#include "kismet/GameplayStatics.h"
#include "MyCharacter/MotionControllerPC.h"
#include "HandMotionController/LeftHandMotionController.h"
#include "Item/PotionBag.h"

// このオブジェクトを手に取って顔に移動すれば体力回復

// Sets default values
APotion::APotion()
{
	// ルートを生成、適用するコード
	Mesh = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("PotionMesh"));
	SetRootComponent(Mesh);	

	// エディタ上でメッシュを探して適用
	static ConstructorHelpers::FObjectFinder<UStaticMesh>SM_Potion(TEXT("StaticMesh'/Game/Assets/CharacterEquipment/Item/Potion/Mesh/potion.potion'"));
	if (SM_Potion.Succeeded())
	{
		Mesh->SetStaticMesh(SM_Potion.Object);
	}

	// ポーションを使用する時にパーティクルをエディタ上でメッシュを探して適用
	static ConstructorHelpers::FObjectFinder<UParticleSystem>PT_PotionUseEffect(TEXT("ParticleSystem'/Game/Assets/Effect/Life/PS_GPP_CannonPurple_Explosion.PS_GPP_CannonPurple_Explosion'"));
	if (PT_PotionUseEffect.Succeeded())
	{
		PotionUseEffect = PT_PotionUseEffect.Object;
	}
	
	// 属性/タグ設定
	Mesh->SetCollisionProfileName(TEXT("NoCollision"));	
	Tags.Add(FName(TEXT("Potion")));	
	Tags.Add(FName(TEXT("DisregardForLeftHand")));
}

// Called when the game starts or when spawned
void APotion::BeginPlay()
{
	Super::BeginPlay();

	// オーバーラップイベント設定
	Mesh->OnComponentBeginOverlap.AddDynamic(this, &APotion::OnPotionBeginOverlap);
}

// オーバーラップイベント
void APotion::OnPotionBeginOverlap(UPrimitiveComponent * OverlappedComponent, AActor * OtherActor, UPrimitiveComponent * OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult & SweepResult)
{
	// ゲームの中のプレイヤーコントローラでプレイヤー探す
	AMotionControllerPC* PC = Cast<AMotionControllerPC>(UGameplayStatics::GetPlayerController(GetWorld(), 0));
	
	if (PC)
	{
		AMotionControllerCharacter* MyCharacter = Cast<AMotionControllerCharacter>(PC->GetPawn());
		if (MyCharacter)
		{
			// 地面とオーバーラップ時の破壊
			if (OtherActor->ActorHasTag(TEXT("Land")))
			{
				Destroy();
			}

			// プレイヤーの顔に移動すればエフェクトと共に体力充電
			if (OtherComp->ComponentHasTag("Head"))
			{
				if (PotionUseEffect)
				{
					UGameplayStatics::SpawnEmitterAtLocation(GetWorld(), PotionUseEffect, GetActorLocation());
				}				
				MyCharacter->CurrentHp += 30.0f;
				Destroy();
			}
		}
	}		
}

