// Fill out your copyright notice in the Description page of Project Settings.

#include "PlayerSword.h"
#include "Components/StaticMeshComponent.h"
#include "UObject/ConstructorHelpers.h"
#include "Components/CapsuleComponent.h"
#include "Kismet/GameplayStatics.h"

#include "MyCharacter/MotionControllerCharacter.h"

#include "Engine/StaticMesh.h"

#include "Haptics/HapticFeedbackEffect_Base.h"
#include "MyCharacter/MotionControllerPC.h"
#include "HandMotionController/RightHandMotionController.h"
#include "Monster/Dog/Dog.h"

// プレーヤーの剣です。 右手に持っています。 主席部分は他のプログラマーが作成しました。

// Sets default values
APlayerSword::APlayerSword()
{
	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

	SwordMesh = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("SwordMesh"));
	SetRootComponent(SwordMesh);
	SwordMesh->SetCollisionProfileName(TEXT("NoCollision"));

	static ConstructorHelpers::FObjectFinder<UStaticMesh>SM_Sword(TEXT("StaticMesh'/Game/Assets/CharacterEquipment/Equipment/Sword/Mesh/SM_Sword.SM_Sword'"));
	if (SM_Sword.Succeeded())
	{
		SwordMesh->SetStaticMesh(SM_Sword.Object);
	}

	SwordCollision = CreateDefaultSubobject<UCapsuleComponent>(TEXT("SwordCollision"));
	SwordCollision->SetupAttachment(SwordMesh);	
	SwordCollision->SetCollisionProfileName(TEXT("OverlapAll"));

	SwordCollision->SetRelativeLocation(FVector(0.0f, 90.0f, 0.0f));
	SwordCollision->SetRelativeRotation(FRotator(0.0f, 0.0f, 90.0f));
	SwordCollision->SetRelativeScale3D(FVector(0.75f, 0.75f, 1.6f));
	SwordCollision->ComponentTags.Add(FName(TEXT("PlayerSwordCollision")));

	Timer = 0.0f;		// 持続的攻撃を防ぐための時間です。

	// 태그
	Tags.Add(FName(TEXT("DisregardForLeftHand")));
	Tags.Add(FName(TEXT("DisregardForRightHand")));
}

// Called when the game starts or when spawned
void APlayerSword::BeginPlay()
{
	Super::BeginPlay();

	// オーナー設定
	SwordOwner = Cast<AMotionControllerCharacter>(UGameplayStatics::GetPlayerCharacter(GetWorld(), 0));

	// 攻撃
	if (SwordCollision)
	{
		SwordCollision->OnComponentBeginOverlap.AddDynamic(this, &APlayerSword::OnSwordOverlap);
	}
}

// Called every frame
void APlayerSword::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	Timer += DeltaTime;		// 타이머

	// 速度を探します。
	if (SwordOwner)
	{
		SwordCurrentPosistion = SwordCollision->GetComponentLocation() - GetActorLocation();
		SwordMoveDelta = SwordCurrentPosistion - SwordPreviousPosistion;
		SwordMoveVelocity = SwordMoveDelta / DeltaTime;
		SwordPreviousPosistion = SwordCurrentPosistion;
	}
}

void APlayerSword::OnSwordOverlap(UPrimitiveComponent * OverlappedComp, AActor * OtherActor, UPrimitiveComponent * OtherComp,
	int32 OtherBodyIndex, bool bFromSweep, const FHitResult & SweepResult)
{
	// Monsterアクター時に実行します。
	if (OtherActor->ActorHasTag("Monster"))
	{
		// 持続的攻撃を防ぐための時間です。
		if (Timer >= 0.5f)
		{
			// 一定速度以上の時に実行します。
			if (SwordMoveVelocity.Size() >= 1500)
			{
				Timer = 0.0f;

				// 速度によってダメージを異にします。
				if (SwordMoveVelocity.Size() <= 1500)
				{
					//RumbleRightController(0.25f);
					Damage = 10.0f;
				}
				else
				{
					//RumbleRightController(0.5f);
					Damage = 15.0f;
				}

				UGameplayStatics::ApplyDamage(OtherActor, Damage, UGameplayStatics::GetPlayerController(GetWorld(), 0), this, nullptr);
			}
		}
	}
}

// グラブの有無によって透明度を調節します。
void APlayerSword::ConvertOfOpacity(float opacity)
{
	if (SwordMesh)
	{
		SwordMesh->SetScalarParameterValueOnMaterials(FName(TEXT("SwordOpacity")), opacity);
	}
}

//void APlayerSword::RumbleRightController(float Intensity)
//{
//	AMotionControllerPC* PC = Cast<AMotionControllerPC>(UGameplayStatics::GetPlayerController(GetWorld(), 0));
//
//	if (PC)
//	{
//		ARightHandMotionController* RightHand = Cast<ARightHandMotionController>(GetAttachParentActor());
//
//		if (RightHand)
//		{
//			PC->RumbleHandController(RightHand->Hand, Intensity);
//		}
//	}
//}
