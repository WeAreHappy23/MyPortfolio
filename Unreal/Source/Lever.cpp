// Fill out your copyright notice in the Description page of Project Settings.

#include "Lever.h"
#include "Components/StaticMeshComponent.h"
#include "UObject/ConstructorHelpers.h"					
#include	"MyCharacter/MotionControllerCharacter.h"	
#include "Kismet/KismetMathLibrary.h"
#include "Kismet/GameplayStatics.h"							
#include "HandMotionController/RightHandMotionController.h"
#include "HandMotionController/LeftHandMotionController.h"
#include "Engine/World.h"
#include "Engine/StaticMesh.h"

#include"Components/BoxComponent.h"
#include"Components/StaticMeshComponent.h"
// Sets default values

// キャラクターの手をドアのTransform基準に変えてAtan2で角を探してRotationに適用します。
// 底辺と高さを知る時,ラジアン=atan(高さ/底辺)なので,角度を得ることができます。

ALever::ALever()
{
	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

	Scene = CreateDefaultSubobject<USceneComponent>(TEXT("Scene"));
	Scene->SetupAttachment(RootComponent);

	LeverScene = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("LeverScene"));
	LeverScene->SetupAttachment(Scene);

	Lever = CreateDefaultSubobject<UBoxComponent>(TEXT("Lever"));
	Lever->SetupAttachment(LeverScene);

	Collision = CreateDefaultSubobject<UBoxComponent>(TEXT("Collision"));
	Collision->SetupAttachment(Lever);

	// スタティックメッシュを探します。
	static ConstructorHelpers::FObjectFinder<UStaticMesh>SM_Door(TEXT("StaticMesh'/Game/Assets/MapBuild/RoughMap/Cathedral_door/backup/Main_door.Main_door'"));	
	if (SM_Door.Succeeded())		
	{
		LeverScene->SetStaticMesh(SM_Door.Object);		
	}

	// 各コンポーネントの大きさと位置を設定します。
	Scene->SetRelativeScale3D(FVector(0.117701f, 0.117701f, 0.117701f));

	LeverScene->SetRelativeScale3D(FVector(4.0f, 4.0f, 4.0f));

	Lever->SetRelativeLocation(FVector(-99.801483f, 2.628052f, 191.0f));
	Lever->SetRelativeScale3D(FVector(0.318f, 0.787583f, 1.380393f));

	Collision->SetRelativeLocation(FVector(148.561737f, 0.259847f, 15.835767f));
	Collision->SetRelativeScale3D(FVector(4.620687f, 0.188942f, 3.429634f));

	// 自動的に開く位置です。
	AutoRot = FRotator(LeverScene->RelativeRotation.Pitch, LeverScene->RelativeRotation.Yaw + 85.0f, LeverScene->RelativeRotation.Roll);
	DefaultYaw = LeverScene->GetComponentRotation().Yaw;

	Collision->bGenerateOverlapEvents = false;

	// 各コンポーネントのコリジョンの属性を設定します。
	LeverScene->SetCollisionProfileName("NoCollision");		// 重なる時,コリジョンなし
	Lever->SetCollisionProfileName("OverlapAll");				// 重なる時,オーバーラップイベント
	Collision->SetCollisionProfileName("BlockAll");				// 重なる時,ブロック

	Tags.Add(FName("Door"));
}

// Called when the game starts or when spawned
void ALever::BeginPlay()
{
	Super::BeginPlay();

	Lever->OnComponentBeginOverlap.AddDynamic(this, &ALever::OnLeverOverlap);	
	Lever->OnComponentEndOverlap.AddDynamic(this, &ALever::OnLeverEndOverlap);
}

// Called every frame
void ALever::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	// タッチされたアクターがある時に開くことができます。
	if (TouchActor)
	{
		ARightHandMotionController* RightHand = Cast<ARightHandMotionController>(TouchActor);
		ALeftHandMotionController* LeftHand = Cast<ALeftHandMotionController>(TouchActor);
		if (RightHand)
		{
			if (RightHand->bisRightGrab)
			{
				// 手をこのアクターのTransformに変換後,手の位置によって変わります。
				FVector Cal = UKismetMathLibrary::InverseTransformLocation
				(GetActorTransform(), RightHand->GetActorLocation());

				float degree = UKismetMathLibrary::RadiansToDegrees(UKismetMathLibrary::Atan2(-Cal.Y, -Cal.X));

				// 角度適用
				if (degree > 0.0f)
					LeverScene->SetRelativeRotation(FRotator(0.0f, degree, 0.0f));
			}
		}
		else if (LeftHand)
		{
			if (LeftHand->bisLeftGrab)
			{
				// 手をこのアクターのTransformに変換後,手の位置によって変わります。
				FVector Cal = UKismetMathLibrary::InverseTransformLocation
				(GetActorTransform(), LeftHand->GetActorLocation());

				float degree = UKismetMathLibrary::RadiansToDegrees(UKismetMathLibrary::Atan2(-Cal.Y, -Cal.X));

				// 角度適用
				if (degree > 0.0f)
					LeverScene->SetRelativeRotation(FRotator(0.0f, degree, 0.0f));
			}
		}
	}

	// 角度が10以上になると自動的に特定地点まで角度が変わります。
	if (LeverScene->RelativeRotation.Yaw > 10.0f)
	{
		LeverScene->SetRelativeRotation(FMath::Lerp(LeverScene->RelativeRotation, AutoRot, 0.01f));
	}

}

// オーバーラップされたアクターが左/右手の時,TouchActorに値が入ってきます。
void ALever::OnLeverOverlap(UPrimitiveComponent * OverlappedComp, AActor * OtherActor, UPrimitiveComponent * OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult & SweepResult)
{
	if (OtherActor->ActorHasTag("RightHand"))
	{
		AMotionControllerCharacter* Character = Cast<AMotionControllerCharacter>(UGameplayStatics::GetPlayerCharacter(GetWorld(), 0));
		if (Character)
		{
			ARightHandMotionController* RightHand = Cast<ARightHandMotionController>(OtherActor);

			if (RightHand)
				TouchActor = Character->RightHand;
		}
	}

	if (OtherActor->ActorHasTag("LeftHand"))
	{
		AMotionControllerCharacter* Character = Cast<AMotionControllerCharacter>(UGameplayStatics::GetPlayerCharacter(GetWorld(), 0));
		if (Character)
		{
			ALeftHandMotionController* RightHand = Cast<ALeftHandMotionController>(OtherActor);

			if (RightHand)
				TouchActor = Character->RightHand;
		}
	}
}

void ALever::OnLeverEndOverlap(UPrimitiveComponent * OverlappedComponent, AActor * OtherActor, UPrimitiveComponent * OtherComp, int32 OtherBodyIndex)
{

}

